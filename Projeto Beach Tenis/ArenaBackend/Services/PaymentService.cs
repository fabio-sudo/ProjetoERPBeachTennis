using ArenaBackend.Data;
using ArenaBackend.DTOs;
using ArenaBackend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaBackend.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ArenaDbContext _context;

        public PaymentService(ArenaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StudentPaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _context.StudentPayments
                .Include(p => p.StudentSubscription)
                    .ThenInclude(ss => ss!.Student)
                .Include(p => p.StudentSubscription)
                    .ThenInclude(ss => ss!.Plan)
                .OrderByDescending(p => p.DueDate)
                .ToListAsync();

            return payments.Select(MapToDto);
        }

        public async Task<IEnumerable<StudentPaymentDto>> GetPaymentsByStudentIdAsync(int studentId)
        {
            var payments = await _context.StudentPayments
                .Include(p => p.StudentSubscription)
                    .ThenInclude(ss => ss!.Student)
                .Include(p => p.StudentSubscription)
                    .ThenInclude(ss => ss!.Plan)
                .Where(p => p.StudentSubscription != null && p.StudentSubscription.StudentId == studentId)
                .OrderByDescending(p => p.DueDate)
                .ToListAsync();

            return payments.Select(MapToDto);
        }

        public async Task<bool> ProcessPaymentAsync(PaymentProcessDto dto)
        {
            var payment = await _context.StudentPayments.FindAsync(dto.PaymentId);
            if (payment == null) return false;

            if (payment.Status == "Paid") return true; // Already paid

            payment.Status = "Paid";
            payment.PaymentDate = DateTime.Now;
            payment.PaymentMethod = dto.PaymentMethod;
            payment.ReceivedByUserName = dto.UserName;

            // Load subscription to get plan name
            if (payment.StudentSubscription == null || payment.StudentSubscription.Plan == null)
            {
                await _context.Entry(payment).Reference(p => p.StudentSubscription).LoadAsync();
                if (payment.StudentSubscription != null)
                {
                    await _context.Entry(payment.StudentSubscription).Reference(ss => ss.Plan).LoadAsync();
                }
            }

            var planName = payment.StudentSubscription?.Plan?.Name ?? "N/A";

            // Find an open cash register
            var openRegister = await _context.CashRegisters.FirstOrDefaultAsync(cr => cr.Status == "Open");
            if (openRegister != null)
            {
                var transaction = new CashTransaction
                {
                    CashRegisterId = openRegister.Id,
                    Type = "Sale",
                    Amount = payment.Amount,
                    Description = $"Mensalidade Aluno - Plano {planName} ({dto.PaymentMethod})",
                    UserName = dto.UserName,
                    CreatedAt = DateTime.Now
                };
                _context.CashTransactions.Add(transaction);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelPaymentAsync(int paymentId, PaymentCancelDto dto)
        {
            var payment = await _context.StudentPayments.FindAsync(paymentId);
            if (payment == null) return false;

            if (payment.Status == "Paid")
            {
                var openRegister = await _context.CashRegisters.FirstOrDefaultAsync(cr => cr.Status == "Open");
                if (openRegister != null)
                {
                    var transaction = new CashTransaction
                    {
                        CashRegisterId = openRegister.Id,
                        Type = "Expense",
                        Amount = payment.Amount,
                        Description = $"Estorno de Mensalidade (#{payment.Id}) - {dto.CancelReason}",
                        UserName = "Sistema",
                        CreatedAt = DateTime.Now
                    };
                    _context.CashTransactions.Add(transaction);
                }
            }

            payment.Status = "Cancelled";
            payment.CancelReason = dto.CancelReason;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<StudentPaymentDto> CreatePaymentAsync(PaymentCreateDto dto)
        {
            // For a manual payment, we need a subscription to attach it to. 
            // If the student has an active subscription, we attach to it.
            var subscription = await _context.StudentSubscriptions
                .Include(s => s.Student)
                .Include(s => s.Plan)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync(s => s.StudentId == dto.StudentId);

            if (subscription == null)
            {
                // Create a dummy Plan and Subscription for standalone payments if none exist
                var dummyPlan = await _context.Plans.FirstOrDefaultAsync(p => p.Name == "Plano Avulso");
                if (dummyPlan == null)
                {
                    dummyPlan = new Plan
                    {
                        Name = "Plano Avulso",
                        Price = 0,
                        DurationDays = 30,
                        Description = "Plano gerado automaticamente para lançamentos avulsos",
                        IsActive = true
                    };
                    _context.Plans.Add(dummyPlan);
                    await _context.SaveChangesAsync();
                }

                subscription = new StudentSubscription
                {
                    StudentId = dto.StudentId,
                    PlanId = dummyPlan.Id,
                    StartDate = DateTime.Now.Date,
                    NextBillingDate = DateTime.Now.Date.AddDays(30),
                    Status = "Active",
                    AutoRenew = false
                };
                _context.StudentSubscriptions.Add(subscription);
                await _context.SaveChangesAsync();
            }

            var payment = new StudentPayment
            {
                StudentSubscriptionId = subscription.Id,
                Amount = dto.Amount,
                DueDate = DateTime.Parse(dto.DueDate),
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.StudentPayments.Add(payment);
            await _context.SaveChangesAsync();

            // Load nav properties for DTO mapping
            await _context.Entry(payment).Reference(p => p.StudentSubscription).LoadAsync();
            if (payment.StudentSubscription != null)
            {
                await _context.Entry(payment.StudentSubscription).Reference(ss => ss.Plan).LoadAsync();
                await _context.Entry(payment.StudentSubscription).Reference(ss => ss.Student).LoadAsync();
            }

            return MapToDto(payment);
        }

        public async Task<StudentPaymentDto?> UpdatePaymentAsync(int id, PaymentUpdateDto dto)
        {
            var payment = await _context.StudentPayments
                .Include(p => p.StudentSubscription)
                    .ThenInclude(ss => ss!.Student)
                .Include(p => p.StudentSubscription)
                    .ThenInclude(ss => ss!.Plan)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return null;

            payment.Amount = dto.Amount;
            payment.DueDate = DateTime.Parse(dto.DueDate);

            // Re-evaluate Overdue/Pending status if it wasn't paid or cancelled
            if (payment.Status == "Pending" || payment.Status == "Overdue")
            {
                if (payment.DueDate < DateTime.Now.Date)
                    payment.Status = "Overdue";
                else
                    payment.Status = "Pending";
            }

            await _context.SaveChangesAsync();
            return MapToDto(payment);
        }

        private StudentPaymentDto MapToDto(StudentPayment p)
        {
            return new StudentPaymentDto
            {
                Id = p.Id,
                StudentSubscriptionId = p.StudentSubscriptionId,
                StudentName = p.StudentSubscription?.Student?.Name ?? "N/A",
                PlanName = p.StudentSubscription?.Plan?.Name ?? "N/A",
                Amount = p.Amount,
                DueDate = p.DueDate.ToString("yyyy-MM-dd"),
                PaymentDate = p.PaymentDate?.ToString("yyyy-MM-dd HH:mm"),
                Status = p.Status,
                PaymentMethod = p.PaymentMethod,
                CancelReason = p.CancelReason,
                ReceivedByUserName = p.ReceivedByUserName
            };
        }
    }
}

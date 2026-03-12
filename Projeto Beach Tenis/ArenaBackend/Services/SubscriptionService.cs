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
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ArenaDbContext _context;

        public SubscriptionService(ArenaDbContext context)
        {
            _context = context;
        }

        public async Task<StudentSubscriptionDto> CreateSubscriptionAsync(SubscriptionCreateDto dto)
        {
            var student = await _context.Students.FindAsync(dto.StudentId);
            if (student == null) throw new Exception("Student not found");

            var plan = await _context.Plans.FindAsync(dto.PlanId);
            if (plan == null) throw new Exception("Plan not found");

            var subscription = new StudentSubscription
            {
                StudentId = dto.StudentId,
                PlanId = dto.PlanId,
                StartDate = DateTime.Now.Date,
                NextBillingDate = DateTime.Now.Date.AddDays(plan.DurationDays),
                Status = "Active",
                AutoRenew = dto.AutoRenew,
                CreatedAt = DateTime.Now
            };

            _context.StudentSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // Generate first payment
            var payment = new StudentPayment
            {
                StudentSubscriptionId = subscription.Id,
                Amount = plan.Price,
                DueDate = DateTime.Now.Date,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.StudentPayments.Add(payment);
            await _context.SaveChangesAsync();

            // Format to return
            return await MapToDtoAsync(subscription);
        }

        public async Task<IEnumerable<StudentSubscriptionDto>> GetAllSubscriptionsAsync()
        {
            var subs = await _context.StudentSubscriptions
                .Include(s => s.Student)
                .Include(s => s.Plan)
                .ToListAsync();

            return subs.Select(s => MapToDto(s));
        }

        public async Task<IEnumerable<StudentSubscriptionDto>> GetSubscriptionsByStudentIdAsync(int studentId)
        {
            var subs = await _context.StudentSubscriptions
                .Include(s => s.Student)
                .Include(s => s.Plan)
                .Where(s => s.StudentId == studentId)
                .ToListAsync();

            return subs.Select(s => MapToDto(s));
        }

        public async Task<bool> CancelSubscriptionAsync(int subscriptionId)
        {
            var subscription = await _context.StudentSubscriptions.FindAsync(subscriptionId);
            if (subscription == null) return false;

            subscription.Status = "Cancelled";
            subscription.AutoRenew = false;
            await _context.SaveChangesAsync();
            return true;
        }

        private StudentSubscriptionDto MapToDto(StudentSubscription s)
        {
            return new StudentSubscriptionDto
            {
                Id = s.Id,
                StudentId = s.StudentId,
                StudentName = s.Student?.Name ?? "N/A",
                PlanId = s.PlanId,
                PlanName = s.Plan?.Name ?? "N/A",
                PlanPrice = s.Plan?.Price ?? 0,
                StartDate = s.StartDate.ToString("yyyy-MM-dd"),
                NextBillingDate = s.NextBillingDate.ToString("yyyy-MM-dd"),
                Status = s.Status,
                AutoRenew = s.AutoRenew
            };
        }

        private async Task<StudentSubscriptionDto> MapToDtoAsync(StudentSubscription subscription)
        {
            // Fully load navigation properties if they are null
            if (subscription.Student == null || subscription.Plan == null)
            {
                var loaded = await _context.StudentSubscriptions
                    .Include(s => s.Student)
                    .Include(s => s.Plan)
                    .FirstOrDefaultAsync(s => s.Id == subscription.Id);

                if (loaded != null) subscription = loaded;
            }

            return MapToDto(subscription);
        }
    }
}

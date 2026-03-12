using ArenaBackend.Data;
using ArenaBackend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaBackend.Services
{
    public class BillingService : IBillingService
    {
        private readonly ArenaDbContext _context;

        public BillingService(ArenaDbContext context)
        {
            _context = context;
        }

        public async Task GenerateMonthlyChargesAsync()
        {
            var today = DateTime.Now.Date;

            // Find subscriptions that are Active, AutoRenew = true, and NextBillingDate is today or before
            var subscriptionsToRenew = await _context.StudentSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.Status == "Active" && s.AutoRenew && s.NextBillingDate <= today)
                .ToListAsync();

            foreach (var sub in subscriptionsToRenew)
            {
                if (sub.Plan == null) continue;

                // Create a new StudentPayment
                var payment = new StudentPayment
                {
                    StudentSubscriptionId = sub.Id,
                    Amount = sub.Plan.Price,
                    DueDate = today,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                _context.StudentPayments.Add(payment);

                // Update NextBillingDate
                sub.NextBillingDate = sub.NextBillingDate.AddDays(sub.Plan.DurationDays);
            }

            await _context.SaveChangesAsync();
        }

        public async Task CheckOverduePaymentsAsync()
        {
            var today = DateTime.Now.Date;

            // Find pending payments where DueDate is in the past
            var overduePayments = await _context.StudentPayments
                .Where(p => p.Status == "Pending" && p.DueDate < today)
                .ToListAsync();

            foreach (var payment in overduePayments)
            {
                payment.Status = "Overdue";
            }

            await _context.SaveChangesAsync();
        }
    }
}

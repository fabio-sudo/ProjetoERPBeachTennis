using System;
using System.Collections.Generic;

namespace ArenaBackend.DTOs
{
    public class StudentSubscriptionDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal PlanPrice { get; set; }
        public string StartDate { get; set; } = string.Empty;
        public string NextBillingDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool AutoRenew { get; set; }

        // Expose recent payments summary if needed
        // public List<StudentPaymentDto> Payments { get; set; } = new();
    }
}

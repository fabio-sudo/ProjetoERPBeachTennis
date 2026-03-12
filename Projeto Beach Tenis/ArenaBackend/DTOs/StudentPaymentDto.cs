using System;

namespace ArenaBackend.DTOs
{
    public class StudentPaymentDto
    {
        public int Id { get; set; }
        public int StudentSubscriptionId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string DueDate { get; set; } = string.Empty;
        public string? PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string? CancelReason { get; set; }
        public string? ReceivedByUserName { get; set; }
    }
}

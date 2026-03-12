using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaBackend.Models
{
    public class StudentPayment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentSubscriptionId { get; set; }

        [ForeignKey("StudentSubscriptionId")]
        public StudentSubscription? StudentSubscription { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime DueDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Overdue, Cancelled

        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // PIX, Cash, CreditCard, DebitCard

        [StringLength(255)]
        public string? CancelReason { get; set; }

        [StringLength(100)]
        public string? ReceivedByUserName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

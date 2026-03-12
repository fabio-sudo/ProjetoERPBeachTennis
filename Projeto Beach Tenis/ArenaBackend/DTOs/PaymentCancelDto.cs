using System.ComponentModel.DataAnnotations;

namespace ArenaBackend.DTOs
{
    public class PaymentCancelDto
    {
        [Required]
        [StringLength(255)]
        public string CancelReason { get; set; } = string.Empty;
    }
}

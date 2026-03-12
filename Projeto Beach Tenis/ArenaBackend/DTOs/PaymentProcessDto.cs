using System;
using System.ComponentModel.DataAnnotations;

namespace ArenaBackend.DTOs
{
    public class PaymentProcessDto
    {
        [Required]
        public int PaymentId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;
    }
}

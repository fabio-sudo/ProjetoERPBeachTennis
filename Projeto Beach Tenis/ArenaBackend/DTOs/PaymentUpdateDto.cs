using System;
using System.ComponentModel.DataAnnotations;

namespace ArenaBackend.DTOs
{
    public class PaymentUpdateDto
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string DueDate { get; set; } = string.Empty;
    }
}

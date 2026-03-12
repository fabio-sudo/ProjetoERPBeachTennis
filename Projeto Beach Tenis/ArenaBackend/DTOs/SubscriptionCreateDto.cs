using System;
using System.ComponentModel.DataAnnotations;

namespace ArenaBackend.DTOs
{
    public class SubscriptionCreateDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int PlanId { get; set; }

        public bool AutoRenew { get; set; } = true;
    }
}

using ArenaBackend.DTOs;
using ArenaBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArenaBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            var subs = await _subscriptionService.GetAllSubscriptionsAsync();
            return Ok(subs);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetSubscriptionsByStudent(int studentId)
        {
            var subs = await _subscriptionService.GetSubscriptionsByStudentIdAsync(studentId);
            return Ok(subs);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var created = await _subscriptionService.CreateSubscriptionAsync(dto);
                return CreatedAtAction(nameof(GetAllSubscriptions), null, created); // Adjust URI logic if needed
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelSubscription(int id)
        {
            var success = await _subscriptionService.CancelSubscriptionAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}

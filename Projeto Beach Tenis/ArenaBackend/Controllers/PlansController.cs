using ArenaBackend.DTOs;
using ArenaBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ArenaBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlansController : ControllerBase
    {
        private readonly IPlanService _planService;

        public PlansController(IPlanService planService)
        {
            _planService = planService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPlans()
        {
            var plans = await _planService.GetAllPlansAsync();
            return Ok(plans);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlanById(int id)
        {
            var plan = await _planService.GetPlanByIdAsync(id);
            if (plan == null) return NotFound();
            return Ok(plan);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlan([FromBody] PlanDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _planService.CreatePlanAsync(dto);
            return CreatedAtAction(nameof(GetPlanById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlan(int id, [FromBody] PlanDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _planService.UpdatePlanAsync(id, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var success = await _planService.DeletePlanAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}

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
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetPaymentsByStudent(int studentId)
        {
            var payments = await _paymentService.GetPaymentsByStudentIdAsync(studentId);
            return Ok(payments);
        }

        [HttpPost("pay")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentProcessDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _paymentService.ProcessPaymentAsync(dto);
            if (!success) return BadRequest(new { Message = "Could not process payment or payment not found." });

            return Ok(new { Message = "Payment processed successfully." });
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelPayment(int id, [FromBody] PaymentCancelDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _paymentService.CancelPaymentAsync(id, dto);
            if (!success) return BadRequest(new { Message = "Could not cancel payment. It may already be paid or not exist." });

            return Ok(new { Message = "Payment cancelled successfully." });
        }

        [HttpPost("manual")]
        public async Task<IActionResult> CreateManualPayment([FromBody] PaymentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var payment = await _paymentService.CreatePaymentAsync(dto);
                return CreatedAtAction(nameof(GetAllPayments), null, payment);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] PaymentUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _paymentService.UpdatePaymentAsync(id, dto);
            if (updated == null) return NotFound(new { Message = "Payment not found." });

            return Ok(updated);
        }
    }
}

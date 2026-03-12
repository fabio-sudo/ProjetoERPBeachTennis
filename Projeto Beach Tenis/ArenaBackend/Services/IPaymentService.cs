using ArenaBackend.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArenaBackend.Services
{
    public interface IPaymentService
    {
        Task<IEnumerable<StudentPaymentDto>> GetAllPaymentsAsync();
        Task<IEnumerable<StudentPaymentDto>> GetPaymentsByStudentIdAsync(int studentId);
        Task<bool> ProcessPaymentAsync(PaymentProcessDto dto);
        Task<bool> CancelPaymentAsync(int paymentId, PaymentCancelDto dto);
        Task<StudentPaymentDto> CreatePaymentAsync(PaymentCreateDto dto);
        Task<StudentPaymentDto?> UpdatePaymentAsync(int id, PaymentUpdateDto dto);
    }
}

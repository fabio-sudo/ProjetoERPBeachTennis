using ArenaBackend.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArenaBackend.Services
{
    public interface ISubscriptionService
    {
        Task<StudentSubscriptionDto> CreateSubscriptionAsync(SubscriptionCreateDto dto);
        Task<IEnumerable<StudentSubscriptionDto>> GetAllSubscriptionsAsync();
        Task<IEnumerable<StudentSubscriptionDto>> GetSubscriptionsByStudentIdAsync(int studentId);
        Task<bool> CancelSubscriptionAsync(int subscriptionId);
    }
}

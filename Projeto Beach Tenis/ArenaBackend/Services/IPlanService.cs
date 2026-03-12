using ArenaBackend.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArenaBackend.Services
{
    public interface IPlanService
    {
        Task<IEnumerable<PlanDto>> GetAllPlansAsync();
        Task<PlanDto?> GetPlanByIdAsync(int id);
        Task<PlanDto> CreatePlanAsync(PlanDto planDto);
        Task<PlanDto?> UpdatePlanAsync(int id, PlanDto planDto);
        Task<bool> DeletePlanAsync(int id);
    }
}

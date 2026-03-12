using ArenaBackend.DTOs;
using ArenaBackend.Models;
using ArenaBackend.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaBackend.Services
{
    public class PlanService : IPlanService
    {
        private readonly IRepository<Plan> _planRepository;

        public PlanService(IRepository<Plan> planRepository)
        {
            _planRepository = planRepository;
        }

        public async Task<IEnumerable<PlanDto>> GetAllPlansAsync()
        {
            var plans = await _planRepository.GetAllAsync();
            return plans.Select(p => new PlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                DurationDays = p.DurationDays,
                Description = p.Description,
                IsActive = p.IsActive
            });
        }

        public async Task<PlanDto?> GetPlanByIdAsync(int id)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            if (plan == null) return null;

            return new PlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Price = plan.Price,
                DurationDays = plan.DurationDays,
                Description = plan.Description,
                IsActive = plan.IsActive
            };
        }

        public async Task<PlanDto> CreatePlanAsync(PlanDto planDto)
        {
            var plan = new Plan
            {
                Name = planDto.Name,
                Price = planDto.Price,
                DurationDays = planDto.DurationDays,
                Description = planDto.Description,
                IsActive = planDto.IsActive
            };

            await _planRepository.AddAsync(plan);
            await _planRepository.SaveChangesAsync();

            planDto.Id = plan.Id;
            return planDto;
        }

        public async Task<PlanDto?> UpdatePlanAsync(int id, PlanDto planDto)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            if (plan == null) return null;

            plan.Name = planDto.Name;
            plan.Price = planDto.Price;
            plan.DurationDays = planDto.DurationDays;
            plan.Description = planDto.Description;
            plan.IsActive = planDto.IsActive;

            await _planRepository.UpdateAsync(plan);
            await _planRepository.SaveChangesAsync();

            return planDto;
        }

        public async Task<bool> DeletePlanAsync(int id)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            if (plan == null) return false;

            await _planRepository.DeleteAsync(id);
            await _planRepository.SaveChangesAsync();
            return true;
        }
    }
}

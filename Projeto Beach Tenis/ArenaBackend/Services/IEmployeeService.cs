using ArenaBackend.DTOs;

namespace ArenaBackend.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeResponseDto>> GetAllAsync();
        Task<EmployeeResponseDto?> GetByIdAsync(int id);
        Task<EmployeeResponseDto> CreateAsync(EmployeeCreateDto dto);
        Task<EmployeeResponseDto?> UpdateAsync(int id, EmployeeUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

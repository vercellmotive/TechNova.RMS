using System;
using System.Threading.Tasks;
using TechNova.RMS.Application.DTOs;

namespace TechNova.RMS.Application.Interfaces.Repositories
{
    public interface IEmployeeRepository
    {
        Task<EmployeeDto> AddAsync(EmployeeDto employee);
        Task<EmployeeDto> UpdateAsync(EmployeeDto employee);
        Task SetActiveStateAsync(Guid employeeId, bool isActive);
        Task<EmployeeDto?> GetByIdAsync(Guid employeeId);
    }
}

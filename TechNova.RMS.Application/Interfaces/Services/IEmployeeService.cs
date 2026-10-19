using System;
using System.Threading.Tasks;
using TechNova.RMS.Application.DTOs;
using TechNova.RMS.Models;

namespace TechNova.RMS.Application.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<ServiceResult<EmployeeDto>> AddAsync(EmployeeDto employee);
        Task<ServiceResult<EmployeeDto>> UpdateAsync(EmployeeDto employee);
        Task SetActiveStateAsync(Guid employeeId, bool isActive);
        Task<EmployeeDto> GetByIdAsync(Guid employeeId);
    }
}

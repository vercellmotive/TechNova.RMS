using System;
using System.Threading.Tasks;
using TechNova.RMS.Application.DTOs;
using TechNova.RMS.Application.Interfaces.Repositories;
using TechNova.RMS.Application.Interfaces.Services;
using TechNova.RMS.Models;

namespace TechNova.RMS.Application.Services
{
    /// <summary>
    /// Application service implementing business use-cases for Employee.
    /// Reason: Coordinates validation and business rules, and delegates persistence to IEmployeeRepository.
    /// </summary>
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;

        public EmployeeService(IEmployeeRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ServiceResult<EmployeeDto>> AddAsync(EmployeeDto employee)
        {
            if (employee == null) throw new ArgumentNullException(nameof(employee));
            // Simple validation example
            if (string.IsNullOrWhiteSpace(employee.FirstName)) throw new ArgumentException("FirstName is required");
            if (string.IsNullOrWhiteSpace(employee.LastName)) throw new ArgumentException("LastName is required");

            // Business rule could go here (e.g., check email uniqueness) — repository can support queries for that.
            var result = await _repository.AddAsync(employee);
            return ServiceResult<EmployeeDto>.Success(result);
        }

        public async Task<ServiceResult<EmployeeDto>> UpdateAsync(EmployeeDto employee)
        {
            if (employee == null) throw new ArgumentNullException(nameof(employee));
            if (employee.Id == Guid.Empty) throw new ArgumentException("Employee Id is required for update");

            var existing = await _repository.GetByIdAsync(employee.Id);
            if (existing == null) throw new InvalidOperationException("Employee not found");

            // Merge or validate fields as required by business
            var result = await _repository.UpdateAsync(employee);
            return ServiceResult<EmployeeDto>.Success(result);
        }

        public async Task SetActiveStateAsync(Guid employeeId, bool isActive)
        {
            if (employeeId == Guid.Empty) throw new ArgumentException("employeeId must be provided", nameof(employeeId));
            var existing = await _repository.GetByIdAsync(employeeId);
            if (existing == null) throw new InvalidOperationException("Employee not found");

            // Business rule could validate transitions (e.g., cannot deactivate certain roles)
            await _repository.SetActiveStateAsync(employeeId, isActive);
        }

        public async Task<EmployeeDto> GetByIdAsync(Guid employeeId)
        {
            // Await to ensure asynchronous flow bubbles up to callers
            return await _repository.GetByIdAsync(employeeId);
        }
    }
}


using System;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.Infrastructure.Data;
using TechNova.RMS.Application.DTOs;
using TechNova.RMS.Application.Interfaces.Repositories;
using TechNova.RMS.Domain.Entities;

namespace TechNova.RMS.Infrastructure.Repositories
{
    /// <summary>
    /// JSON-backed implementation of IEmployeeRepository.
    /// Uses IAppDbContext (JsonDbContext) as the persistence mechanism so higher layers remain decoupled.
    /// </summary>
    public class EmployeeRepository : BaseRepository, IEmployeeRepository
    {
        public EmployeeRepository(IAppDbContext db) : base(db)
        {
        }

        public Task<EmployeeDto> AddAsync(EmployeeDto employee)
        {
            if (employee == null) throw new ArgumentNullException(nameof(employee));

            var domain = MapToDomain(employee);
            domain.Id = Guid.NewGuid().ToString();
            _db.Employees.Add(domain);
            _db.SaveChanges();

            var dto = MapToDto(domain);
            return Task.FromResult(dto);
        }

        public Task<EmployeeDto> UpdateAsync(EmployeeDto employee)
        {
            if (employee == null) throw new ArgumentNullException(nameof(employee));
            var idString = employee.Id.ToString();
            var existing = _db.Employees.FirstOrDefault(e => e.Id == idString);
            if (existing == null) throw new InvalidOperationException("Employee not found");

            // simple field updates
            existing.Name = string.Join(' ', employee.FirstName, employee.LastName).Trim();
            existing.Email = employee.Email;
            existing.Designation = employee.Designation;
            existing.Department = employee.Department;
            existing.EmployementType = employee.EmployementType;
            existing.Salary = employee.Salary;
            existing.IsActive = employee.IsActive;

            // replace skills with simple string list
            existing.Skills.Clear();
            foreach (var s in employee.Skills ?? Enumerable.Empty<string>())
            {
                existing.Skills.Add(new Domain.Models.EmployeeSkill { Name = s, Description = string.Empty, ProficiencyLevel = string.Empty });
            }

            _db.SaveChanges();

            return Task.FromResult(MapToDto(existing));
        }

        public Task SetActiveStateAsync(Guid employeeId, bool isActive)
        {
            var idString = employeeId.ToString();
            var existing = _db.Employees.FirstOrDefault(e => e.Id == idString);
            if (existing == null) throw new InvalidOperationException("Employee not found");
            existing.IsActive = isActive;
            _db.SaveChanges();
            return Task.CompletedTask;
        }

        public Task<EmployeeDto?> GetByIdAsync(Guid employeeId)
        {
            var idString = employeeId.ToString();
            var existing = _db.Employees.FirstOrDefault(e => e.Id == idString);
            return Task.FromResult(existing is null ? null : MapToDto(existing));
        }

        private static Employee MapToDomain(EmployeeDto dto)
        {
            var name = string.Join(' ', dto.FirstName, dto.LastName).Trim();
            var emp = new Employee
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid().ToString() : dto.Id.ToString(),
                Name = name,
                Email = dto.Email,
                Designation = dto.Designation,
                Department = dto.Department,
                EmployementType = dto.EmployementType,
                Salary = dto.Salary,
                IsActive = dto.IsActive
            };
            foreach (var s in dto.Skills ?? Enumerable.Empty<string>())
            {
                emp.Skills.Add(new Domain.Models.EmployeeSkill { Name = s, Description = string.Empty, ProficiencyLevel = string.Empty });
            }
            return emp;
        }

        private static EmployeeDto MapToDto(Employee e)
        {
            var names = (e.Name ?? string.Empty).Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var first = names.Length > 0 ? names[0] : string.Empty;
            var last = names.Length > 1 ? names[1] : string.Empty;
            return new EmployeeDto
            {
                Id = Guid.TryParse(e.Id, out var g) ? g : Guid.Empty,
                FirstName = first,
                LastName = last,
                Email = e.Email,
                Designation = e.Designation,
                Department = e.Department,
                EmployementType = e.EmployementType,
                Salary = e.Salary,
                IsActive = e.IsActive,
                Skills = e.Skills?.Select(s => s.Name).ToList() ?? new System.Collections.Generic.List<string>()
            };
        }
    }
}


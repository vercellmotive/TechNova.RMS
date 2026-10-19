using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.Application.DTOs;
using TechNova.RMS.Application.Interfaces.Repositories;
using TechNova.RMS.Application.Interfaces.Services;
using TechNova.RMS.Models;

namespace TechNova.RMS.Application.Services
{
    /// <summary>
    /// Implements allocation use-cases and enforces allocation business rules.
    /// </summary>
    public class AllocationService : IAllocationService
    {
        private readonly IAllocationRepository _allocationRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IProjectRepository _projectRepo;

        public AllocationService(IAllocationRepository allocationRepo, IEmployeeRepository employeeRepo, IProjectRepository projectRepo)
        {
            _allocationRepo = allocationRepo ?? throw new ArgumentNullException(nameof(allocationRepo));
            _employeeRepo = employeeRepo ?? throw new ArgumentNullException(nameof(employeeRepo));
            _projectRepo = projectRepo ?? throw new ArgumentNullException(nameof(projectRepo));
        }

        public async Task<ServiceResult<AllocationDto>> AllocateAsync(AllocationDto allocation)
        {
            if (allocation == null) throw new ArgumentNullException(nameof(allocation));
            if (allocation.StartDate > allocation.EndDate) return ServiceResult<AllocationDto>.Failure("StartDate must be before EndDate");

            // ensure employee exists and is active
            var employee = await _employeeRepo.GetByIdAsync(allocation.EmployeeId);
            if (employee == null) return ServiceResult<AllocationDto>.Failure("Employee not found");
            if (!employee.IsActive) return ServiceResult<AllocationDto>.Failure("Cannot allocate inactive employee");

            // ensure project exists and allocation falls within project dates
            var project = await _projectRepo.GetByIdAsync(allocation.ProjectId);
            if (project == null) return ServiceResult<AllocationDto>.Failure("Project not found");
            if (allocation.StartDate < project.StartDate || allocation.EndDate > project.EndDate)
                return ServiceResult<AllocationDto>.Failure("Allocation must fall within project dates");

            // check overlapping allocations do not exceed 100%
            var existing = await _allocationRepo.GetByEmployeeAsync(allocation.EmployeeId);
            var overlaps = existing.Where(a => RangesOverlap(a.StartDate, a.EndDate, allocation.StartDate, allocation.EndDate));
            var totalPercent = overlaps.Sum(a => a.AllocationPercent) + allocation.AllocationPercent;
            if (totalPercent > 100m) return ServiceResult<AllocationDto>.Failure("Allocation would exceed 100% for overlapping period");

            await _allocationRepo.AddAsync(allocation);

            // simulate notification
            Console.WriteLine($"Notification: Employee {allocation.EmployeeId} allocated to project {allocation.ProjectId} {allocation.AllocationPercent}% from {allocation.StartDate:d} to {allocation.EndDate:d}");

            return ServiceResult<AllocationDto>.Success(allocation);
        }

        public Task<IEnumerable<AllocationDto>> GetAllocationsForEmployeeAsync(Guid employeeId)
        {
            return _allocationRepo.GetByEmployeeAsync(employeeId);
        }

        public Task<IEnumerable<AllocationDto>> GetAllocationsForProjectAsync(string projectId)
        {
            return _allocationRepo.GetByProjectAsync(projectId);
        }

        private static bool RangesOverlap(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        {
            return aStart <= bEnd && bStart <= aEnd;
        }
    }
}

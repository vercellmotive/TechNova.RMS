using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.Application.DTOs;
using TechNova.RMS.Application.Interfaces.Repositories;
using TechNova.RMS.Infrastructure.Data;
using TechNova.RMS.Domain.Models;

namespace TechNova.RMS.Infrastructure.Repositories
{
    public class AllocationRepository : IAllocationRepository
    {
        private readonly IAppDbContext _db;

        public AllocationRepository(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Task AddAsync(AllocationDto allocation)
        {
            var emp = _db.Employees.FirstOrDefault(e => e.Id == allocation.EmployeeId.ToString());
            if (emp == null) throw new InvalidOperationException("Employee not found");

            var domain = new Allocation
            {
                ProjectId = allocation.ProjectId,
                StartDate = allocation.StartDate,
                EndDate = allocation.EndDate,
                AllocationPercent = allocation.AllocationPercent
            };

            emp.Allocations.Add(domain);
            _db.SaveChanges();
            return Task.CompletedTask;
        }

        public Task<IEnumerable<AllocationDto>> GetByEmployeeAsync(Guid employeeId)
        {
            var emp = _db.Employees.FirstOrDefault(e => e.Id == employeeId.ToString());
            if (emp == null) return Task.FromResult(Enumerable.Empty<AllocationDto>());

            var list = emp.Allocations.Select(a => new AllocationDto
            {
                EmployeeId = Guid.TryParse(emp.Id, out var g) ? g : Guid.Empty,
                ProjectId = a.ProjectId,
                StartDate = a.StartDate == default ? (a.Date ?? DateTime.MinValue) : a.StartDate,
                EndDate = a.EndDate == default ? (a.Date ?? DateTime.MinValue) : a.EndDate,
                AllocationPercent = a.AllocationPercent
            }).ToList();

            return Task.FromResult<IEnumerable<AllocationDto>>(list);
        }

        public Task<IEnumerable<AllocationDto>> GetByProjectAsync(string projectId)
        {
            var list = _db.Employees.SelectMany(e => e.Allocations.Select(a => (Employee: e, Allocation: a)))
                .Where(x => x.Allocation.ProjectId == projectId)
                .Select(x => new AllocationDto
                {
                    EmployeeId = Guid.TryParse(x.Employee.Id, out var g) ? g : Guid.Empty,
                    ProjectId = x.Allocation.ProjectId,
                    StartDate = x.Allocation.StartDate == default ? (x.Allocation.Date ?? DateTime.MinValue) : x.Allocation.StartDate,
                    EndDate = x.Allocation.EndDate == default ? (x.Allocation.Date ?? DateTime.MinValue) : x.Allocation.EndDate,
                    AllocationPercent = x.Allocation.AllocationPercent
                }).ToList();

            return Task.FromResult<IEnumerable<AllocationDto>>(list);
        }
    }
}

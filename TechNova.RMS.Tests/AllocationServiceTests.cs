using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.Application.DTOs;
using TechNova.RMS.Application.Interfaces.Repositories;
using TechNova.RMS.Application.Services;
using Xunit;

namespace TechNova.RMS.Tests
{
    public class AllocationServiceTests
    {
        private class InMemoryAllocationRepo : IAllocationRepository
        {
            public readonly List<AllocationDto> Items = new();
            public Task AddAsync(AllocationDto allocation)
            {
                Items.Add(allocation);
                return Task.CompletedTask;
            }

            public Task<IEnumerable<AllocationDto>> GetByEmployeeAsync(Guid employeeId)
            {
                return Task.FromResult<IEnumerable<AllocationDto>>(Items.Where(i => i.EmployeeId == employeeId).ToList());
            }

            public Task<IEnumerable<AllocationDto>> GetByProjectAsync(string projectId)
            {
                return Task.FromResult<IEnumerable<AllocationDto>>(Items.Where(i => i.ProjectId == projectId).ToList());
            }
        }

        private class InMemoryEmployeeRepo : IEmployeeRepository
        {
            private readonly Dictionary<Guid, Application.DTOs.EmployeeDto> _store = new();
            public Task<EmployeeDto> AddAsync(EmployeeDto employee)
            {
                var id = employee.Id == Guid.Empty ? Guid.NewGuid() : employee.Id;
                employee.Id = id;
                _store[id] = employee;
                return Task.FromResult(employee);
            }

            public Task<EmployeeDto?> GetByIdAsync(Guid employeeId)
            {
                _store.TryGetValue(employeeId, out var v);
                return Task.FromResult<EmployeeDto?>(v);
            }

            public Task SetActiveStateAsync(Guid employeeId, bool isActive)
            {
                if (_store.TryGetValue(employeeId, out var v)) v.IsActive = isActive;
                return Task.CompletedTask;
            }

            public Task<EmployeeDto> UpdateAsync(EmployeeDto employee)
            {
                if (!_store.ContainsKey(employee.Id)) throw new InvalidOperationException("Employee not found");
                _store[employee.Id] = employee;
                return Task.FromResult(employee);
            }
        }

        private class InMemoryProjectRepo : IProjectRepository
        {
            private readonly Dictionary<string, ProjectDto> _store = new();
            public void Add(ProjectDto p) => _store[p.Id] = p;
            public Task<ProjectDto?> GetByIdAsync(string projectId)
            {
                _store.TryGetValue(projectId, out var v);
                return Task.FromResult<ProjectDto?>(v);
            }
        }

        [Fact]
        public async Task AllocateAsync_Succeeds_ForValidAllocation()
        {
            var allocRepo = new InMemoryAllocationRepo();
            var empRepo = new InMemoryEmployeeRepo();
            var projRepo = new InMemoryProjectRepo();

            var emp = new EmployeeDto { Id = Guid.NewGuid(), FirstName = "A", LastName = "B", IsActive = true };
            await empRepo.AddAsync(emp);

            var proj = new ProjectDto { Id = "P1", Name = "Proj1", StartDate = DateTime.Today.AddDays(-1), EndDate = DateTime.Today.AddDays(10) };
            projRepo.Add(proj);

            var service = new AllocationService(allocRepo, empRepo, projRepo);

            var dto = new AllocationDto { EmployeeId = emp.Id, ProjectId = proj.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), AllocationPercent = 50m };
            var result = await service.AllocateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
            Assert.Single(allocRepo.Items);
        }

        [Fact]
        public async Task AllocateAsync_Fails_WhenEmployeeInactive()
        {
            var allocRepo = new InMemoryAllocationRepo();
            var empRepo = new InMemoryEmployeeRepo();
            var projRepo = new InMemoryProjectRepo();

            var emp = new EmployeeDto { Id = Guid.NewGuid(), FirstName = "A", LastName = "B", IsActive = false };
            await empRepo.AddAsync(emp);

            var proj = new ProjectDto { Id = "P1", Name = "Proj1", StartDate = DateTime.Today.AddDays(-1), EndDate = DateTime.Today.AddDays(10) };
            projRepo.Add(proj);

            var service = new AllocationService(allocRepo, empRepo, projRepo);
            var dto = new AllocationDto { EmployeeId = emp.Id, ProjectId = proj.Id, StartDate = DateTime.Today, EndDate = DateTime.Today, AllocationPercent = 50m };
            var result = await service.AllocateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task AllocateAsync_Fails_WhenOutsideProjectDates()
        {
            var allocRepo = new InMemoryAllocationRepo();
            var empRepo = new InMemoryEmployeeRepo();
            var projRepo = new InMemoryProjectRepo();

            var emp = new EmployeeDto { Id = Guid.NewGuid(), FirstName = "A", LastName = "B", IsActive = true };
            await empRepo.AddAsync(emp);

            var proj = new ProjectDto { Id = "P1", Name = "Proj1", StartDate = DateTime.Today.AddDays(5), EndDate = DateTime.Today.AddDays(10) };
            projRepo.Add(proj);

            var service = new AllocationService(allocRepo, empRepo, projRepo);
            var dto = new AllocationDto { EmployeeId = emp.Id, ProjectId = proj.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1), AllocationPercent = 50m };
            var result = await service.AllocateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task AllocateAsync_Fails_WhenOverlapExceeds100()
        {
            var allocRepo = new InMemoryAllocationRepo();
            var empRepo = new InMemoryEmployeeRepo();
            var projRepo = new InMemoryProjectRepo();

            var emp = new EmployeeDto { Id = Guid.NewGuid(), FirstName = "A", LastName = "B", IsActive = true };
            await empRepo.AddAsync(emp);

            var proj = new ProjectDto { Id = "P1", Name = "Proj1", StartDate = DateTime.Today.AddDays(-5), EndDate = DateTime.Today.AddDays(10) };
            projRepo.Add(proj);

            // existing allocation 60%
            allocRepo.Items.Add(new AllocationDto { EmployeeId = emp.Id, ProjectId = proj.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), AllocationPercent = 60m });

            var service = new AllocationService(allocRepo, empRepo, projRepo);
            var dto = new AllocationDto { EmployeeId = emp.Id, ProjectId = proj.Id, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(3), AllocationPercent = 50m };
            var result = await service.AllocateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }
    }
}

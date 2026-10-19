using System;
using System.Threading.Tasks;
using TechNova.RMS.Application.DTOs;
using TechNova.RMS.Application.Interfaces.Repositories;
using TechNova.RMS.Application.Services;
using Xunit;

namespace TechNova.RMS.Tests
{
    public class EmployeeServiceTests
    {
        private class InMemoryEmployeeRepo : IEmployeeRepository
        {
            private readonly System.Collections.Generic.Dictionary<Guid, EmployeeDto> _store = new();

            public Task<EmployeeDto> AddAsync(EmployeeDto employee)
            {
                var id = employee.Id == Guid.Empty ? Guid.NewGuid() : employee.Id;
                employee.Id = id;
                _store[id] = employee;
                return Task.FromResult(employee);
            }

            public Task<EmployeeDto> UpdateAsync(EmployeeDto employee)
            {
                if (!_store.ContainsKey(employee.Id)) throw new InvalidOperationException("Employee not found");
                _store[employee.Id] = employee;
                return Task.FromResult(employee);
            }

            public Task SetActiveStateAsync(Guid employeeId, bool isActive)
            {
                if (_store.TryGetValue(employeeId, out var v)) v.IsActive = isActive;
                return Task.CompletedTask;
            }

            public Task<EmployeeDto?> GetByIdAsync(Guid employeeId)
            {
                _store.TryGetValue(employeeId, out var v);
                return Task.FromResult<EmployeeDto?>(v);
            }
        }

        [Fact]
        public async Task AddAsync_CreatesEmployee()
        {
            var repo = new InMemoryEmployeeRepo();
            var svc = new EmployeeService(repo);
            var dto = new EmployeeDto { FirstName = "John", LastName = "Doe", Email = "j@d", IsActive = true };
            var res = await svc.AddAsync(dto);
            Assert.NotNull(res);
            Assert.Equal(System.Net.HttpStatusCode.OK, res.StatusCode);
            Assert.NotEqual(Guid.Empty, res.Data?.Id);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesEmployee()
        {
            var repo = new InMemoryEmployeeRepo();
            var svc = new EmployeeService(repo);
            var dto = new EmployeeDto { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "X", Email = "j@x" };
            await repo.AddAsync(dto);

            dto.LastName = "Y";
            var res = await svc.UpdateAsync(dto);
            Assert.NotNull(res);
            Assert.Equal(System.Net.HttpStatusCode.OK, res.StatusCode);
            Assert.Equal("Y", res.Data?.LastName);
        }

        [Fact]
        public async Task SetActiveStateAsync_ChangesState()
        {
            var repo = new InMemoryEmployeeRepo();
            var svc = new EmployeeService(repo);
            var dto = new EmployeeDto { Id = Guid.NewGuid(), FirstName = "Sam", LastName = "S" };
            await repo.AddAsync(dto);

            await svc.SetActiveStateAsync(dto.Id, false);
            var after = await svc.GetByIdAsync(dto.Id);
            Assert.False(after.IsActive);
        }
    }
}

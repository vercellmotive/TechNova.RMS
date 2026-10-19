using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechNova.RMS.Application.DTOs;

namespace TechNova.RMS.Application.Interfaces.Repositories
{
    public interface IAllocationRepository
    {
        Task AddAsync(AllocationDto allocation);
        Task<IEnumerable<AllocationDto>> GetByEmployeeAsync(Guid employeeId);
        Task<IEnumerable<AllocationDto>> GetByProjectAsync(string projectId);
    }
}

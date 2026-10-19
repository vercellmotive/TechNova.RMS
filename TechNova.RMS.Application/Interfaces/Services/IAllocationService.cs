using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechNova.RMS.Application.DTOs;
using TechNova.RMS.Models;

namespace TechNova.RMS.Application.Interfaces.Services
{
    public interface IAllocationService
    {
        Task<ServiceResult<AllocationDto>> AllocateAsync(AllocationDto allocation);
        Task<IEnumerable<AllocationDto>> GetAllocationsForEmployeeAsync(Guid employeeId);
        Task<IEnumerable<AllocationDto>> GetAllocationsForProjectAsync(string projectId);
    }
}

using System.Threading.Tasks;
using TechNova.RMS.Application.DTOs;

namespace TechNova.RMS.Application.Interfaces.Repositories
{
    public interface IProjectRepository
    {
        Task<ProjectDto?> GetByIdAsync(string projectId);
    }
}

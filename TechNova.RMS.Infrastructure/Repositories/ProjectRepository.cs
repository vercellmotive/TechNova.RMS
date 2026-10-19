using System;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.Application.DTOs;
using TechNova.RMS.Application.Interfaces.Repositories;
using TechNova.RMS.Infrastructure.Data;

namespace TechNova.RMS.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly IAppDbContext _db;

        public ProjectRepository(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Task<ProjectDto?> GetByIdAsync(string projectId)
        {
            var p = _db.Projects.FirstOrDefault(x => x.Id == projectId);
            if (p == null) return Task.FromResult<ProjectDto?>(null);

            var dto = new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                ClientName = p.ClientName,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                Skills = p.Skills?.Select(s => s.Name).ToList() ?? new System.Collections.Generic.List<string>()
            };

            return Task.FromResult<ProjectDto?>(dto);
        }
    }
}

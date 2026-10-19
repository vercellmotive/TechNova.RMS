using System;
using System.Collections.Generic;
using System.Text;
using TechNova.RMS.Domain.Models;

namespace TechNova.RMS.Domain.Entities
{
    public class Project
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;

        public List<ProjectSkill> Skills { get; set; } = [];
    }
}

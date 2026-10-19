using System;
using System.Collections.Generic;
using System.Text;
using TechNova.RMS.Domain.Enum;

namespace TechNova.RMS.Domain.Models
{
    public class EmployeeSkill
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ProficiencyLevel { get; set; } = ProficiencyLevels.Beginner;
    }
}

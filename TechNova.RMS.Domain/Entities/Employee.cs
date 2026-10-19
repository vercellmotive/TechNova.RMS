using System;
using System.Collections.Generic;
using System.Text;
using TechNova.RMS.Domain.Enum;
using TechNova.RMS.Domain.Models;

namespace TechNova.RMS.Domain.Entities
{
    public class Employee
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string EmployementType { get; set; } = EmployementTypes.PartTime;
        public decimal Salary { get; set; }
        public bool IsActive { get; set; } = true;

        public List<Allocation> Allocations { get; set; } = [];
        public List<EmployeeSkill> Skills { get; set; } = [];
    }
}

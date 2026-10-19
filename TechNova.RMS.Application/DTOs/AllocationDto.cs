using System;

namespace TechNova.RMS.Application.DTOs
{
    public class AllocationDto
    {
        public Guid EmployeeId { get; set; }
        public string ProjectId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal AllocationPercent { get; set; }
    }
}

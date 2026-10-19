using System;
using System.Collections.Generic;
using System.Text;

namespace TechNova.RMS.Domain.Models
{
    public class Allocation
    {
        public string ProjectId { get; set; } = string.Empty;
        // New range-based allocation fields to support allocation percentage over a period
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        // Allocation percentage (0-100)
        public decimal AllocationPercent { get; set; } = 0;
        // Back-compat: legacy single-date/hours fields (may be present in seed JSON)
        public DateTime? Date { get; set; }
        public decimal HoursAllocated { get; set; } = 0;
    }
}

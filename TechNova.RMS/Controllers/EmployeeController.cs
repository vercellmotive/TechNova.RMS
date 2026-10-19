using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TechNova.RMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : BaseApiController
    {
        [HttpGet]
        public IActionResult GetEmployees()
        {
            // Sample data for demonstration purposes
            var employees = new[]
            {
                new { Id = 1, Name = "John Doe", Position = "Software Engineer" },
                new { Id = 2, Name = "Jane Smith", Position = "Project Manager" },
                new { Id = 3, Name = "Sam Johnson", Position = "QA Analyst" }
            };
            return HandleResult(employees);
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.Infrastructure.Data;
using TechNova.RMS.Application.Interfaces.Services;
using TechNova.RMS.Application.DTOs;
using System.Collections.Generic;

namespace TechNova.RMS.Cli
{
    /// <summary>
    /// Console application runner. Keeps UI / orchestration separate from Program.cs (composition root)
    /// and delegates business concerns to application services.
    /// </summary>
    public class ConsoleApp
    {
        private readonly IAppDbContext _db;
        private readonly IAllocationService _allocationService;
        private readonly IEmployeeService _employeeService;

        public ConsoleApp(IAppDbContext db, IAllocationService allocationService, IEmployeeService employeeService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _allocationService = allocationService ?? throw new ArgumentNullException(nameof(allocationService));
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        }

        public async Task RunAsync()
        {
            Console.WriteLine("TechNova RMS - Console Workflow Demo\n");

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Menu:");
                Console.WriteLine("1) List employees");
                Console.WriteLine("2) Create employee");
                Console.WriteLine("3) Update employee");
                Console.WriteLine("4) Set employee active/inactive");
                Console.WriteLine("5) Create project");
                Console.WriteLine("6) Allocation report (date range)");
                Console.WriteLine("7) Allocate project to employee");
                Console.WriteLine("0) Exit");
                Console.Write("Select an option: ");
                var key = Console.ReadLine()?.Trim();
                try
                {
                    switch (key)
                    {
                        case "1":
                            ListEmployees();
                            break;
                        case "2":
                            await CreateEmployeeAsync();
                            break;
                        case "3":
                            await UpdateEmployeeAsync();
                            break;
                        case "4":
                            await SetEmployeeActiveStateAsync();
                            break;
                        case "5":
                            CreateProject();
                            break;
                        case "6":
                            await ShowAllocationReportAsync();
                            break;
                        case "7":
                            await AllocateProjectAsync();
                            break;
                        case "0":
                            return;
                        default:
                            Console.WriteLine("Unknown option");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private void ListEmployees()
        {
            Console.WriteLine($"Employees ({_db.Employees.Count}):");
            for (int i = 0; i < _db.Employees.Count; i++)
            {
                var e = _db.Employees[i];
                Console.WriteLine($"[{i}] {e.Name} Id={e.Id} Active={e.IsActive} Dept={e.Department} Skills={string.Join(',', e.Skills.Select(s => s.Name))}");
            }
        }

        private async Task CreateEmployeeAsync()
        {
            Console.WriteLine("Create new employee");
            Console.Write("First name: "); var first = Console.ReadLine()?.Trim() ?? string.Empty;
            Console.Write("Last name: "); var last = Console.ReadLine()?.Trim() ?? string.Empty;
            Console.Write("Email: "); var email = Console.ReadLine()?.Trim() ?? string.Empty;
            Console.Write("Designation: "); var desig = Console.ReadLine()?.Trim() ?? string.Empty;
            Console.Write("Department: "); var dept = Console.ReadLine()?.Trim() ?? string.Empty;
            Console.Write("Employment type: "); var etype = Console.ReadLine()?.Trim() ?? string.Empty;
            Console.Write("Salary: "); decimal.TryParse(Console.ReadLine(), out var salary);
            Console.Write("Skills (comma separated): "); var skillsInput = Console.ReadLine() ?? string.Empty;
            var skills = skillsInput.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => s.Length > 0).ToList();

            var dto = new EmployeeDto
            {
                Id = Guid.NewGuid(),
                FirstName = first,
                LastName = last,
                Email = email,
                Designation = desig,
                Department = dept,
                EmployementType = etype,
                Salary = salary,
                IsActive = true,
                Skills = skills
            };

            var res = await _employeeService.AddAsync(dto);
            if (res == null || res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Failed to create employee");
            }
            else
            {
                Console.WriteLine($"Employee created: {res.Data?.FirstName} {res.Data?.LastName} Id={res.Data?.Id}");
            }
        }

        private async Task UpdateEmployeeAsync()
        {
            Console.WriteLine("Select employee to update:");
            ListEmployees();
            Console.Write("Enter index: ");
            if (!int.TryParse(Console.ReadLine(), out var idx) || idx < 0 || idx >= _db.Employees.Count)
            {
                Console.WriteLine("Invalid selection");
                return;
            }
            var domain = _db.Employees[idx];
            var id = Guid.TryParse(domain.Id, out var gid) ? gid : Guid.Empty;
            var existing = await _employeeService.GetByIdAsync(id);
            if (existing == null) { Console.WriteLine("Employee not found"); return; }

            Console.Write($"First name ({existing.FirstName}): "); var first = Console.ReadLine();
            Console.Write($"Last name ({existing.LastName}): "); var last = Console.ReadLine();
            Console.Write($"Email ({existing.Email}): "); var email = Console.ReadLine();
            Console.Write($"Designation ({existing.Designation}): "); var desig = Console.ReadLine();
            Console.Write($"Department ({existing.Department}): "); var dept = Console.ReadLine();
            Console.Write($"Employment type ({existing.EmployementType}): "); var etype = Console.ReadLine();
            Console.Write($"Salary ({existing.Salary}): "); var salaryInput = Console.ReadLine();
            Console.Write($"Skills ({string.Join(',', existing.Skills)}): "); var skillsInput = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(first)) existing.FirstName = first.Trim();
            if (!string.IsNullOrWhiteSpace(last)) existing.LastName = last.Trim();
            if (!string.IsNullOrWhiteSpace(email)) existing.Email = email.Trim();
            if (!string.IsNullOrWhiteSpace(desig)) existing.Designation = desig.Trim();
            if (!string.IsNullOrWhiteSpace(dept)) existing.Department = dept.Trim();
            if (!string.IsNullOrWhiteSpace(etype)) existing.EmployementType = etype.Trim();
            if (decimal.TryParse(salaryInput, out var sal)) existing.Salary = sal;
            if (!string.IsNullOrWhiteSpace(skillsInput)) existing.Skills = skillsInput.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            var updateDto = existing;
            var res = await _employeeService.UpdateAsync(updateDto);
            if (res == null || res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Update failed");
            }
            else
            {
                Console.WriteLine("Employee updated");
            }
        }

        private async Task SetEmployeeActiveStateAsync()
        {
            Console.WriteLine("Select employee to change active state:");
            ListEmployees();
            Console.Write("Enter index: ");
            if (!int.TryParse(Console.ReadLine(), out var idx) || idx < 0 || idx >= _db.Employees.Count)
            {
                Console.WriteLine("Invalid selection");
                return;
            }
            var domain = _db.Employees[idx];
            var id = Guid.TryParse(domain.Id, out var gid) ? gid : Guid.Empty;
            Console.WriteLine($"Current state: {domain.IsActive}. Set active? (y/n): ");
            var resp = Console.ReadLine();
            var setActive = !string.IsNullOrWhiteSpace(resp) && resp.Trim().ToLowerInvariant().StartsWith("y");
            await _employeeService.SetActiveStateAsync(id, setActive);
            Console.WriteLine("Employee state updated");
        }

        private void CreateProject()
        {
            Console.WriteLine("Create new project");
            Console.Write("Name: "); var name = Console.ReadLine() ?? string.Empty;
            Console.Write("Client name: "); var client = Console.ReadLine() ?? string.Empty;
            Console.Write("Start date (yyyy-MM-dd): "); if (!DateTime.TryParse(Console.ReadLine(), out var start)) { Console.WriteLine("Invalid date"); return; }
            Console.Write("End date (yyyy-MM-dd): "); if (!DateTime.TryParse(Console.ReadLine(), out var end)) { Console.WriteLine("Invalid date"); return; }
            Console.Write("Status: "); var status = Console.ReadLine() ?? string.Empty;
            Console.Write("Skills (comma separated): "); var skillsInput = Console.ReadLine() ?? string.Empty;
            var skills = skillsInput.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => s.Length > 0).ToList();

            var proj = new TechNova.RMS.Domain.Entities.Project
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                ClientName = client,
                StartDate = start.Date,
                EndDate = end.Date,
                Status = status
            };
            foreach (var s in skills) proj.Skills.Add(new TechNova.RMS.Domain.Models.ProjectSkill { Name = s });
            _db.Projects.Add(proj);
            _db.SaveChanges();
            Console.WriteLine($"Project created: {proj.Name} Id={proj.Id}");
        }

        private async Task AllocateProjectAsync()
        {
            Console.WriteLine("Allocate project to employee");
            // select employee
            Console.WriteLine("Select employee:");
            ListEmployees();
            Console.Write("Enter employee index: ");
            if (!int.TryParse(Console.ReadLine(), out var empIndex) || empIndex < 0 || empIndex >= _db.Employees.Count)
            {
                Console.WriteLine("Invalid employee selection");
                return;
            }
            var employee = _db.Employees[empIndex];
            if (!employee.IsActive)
            {
                Console.WriteLine("Employee is inactive and cannot be allocated.");
                return;
            }

            // select project
            Console.WriteLine("Select project:");
            for (int i = 0; i < _db.Projects.Count; i++)
            {
                var p = _db.Projects[i];
                Console.WriteLine($"[{i}] {p.Name} (Id={p.Id}) {p.StartDate:d} - {p.EndDate:d}");
            }
            Console.Write("Enter project index: ");
            if (!int.TryParse(Console.ReadLine(), out var projIndex) || projIndex < 0 || projIndex >= _db.Projects.Count)
            {
                Console.WriteLine("Invalid project selection");
                return;
            }
            var project = _db.Projects[projIndex];

            // read dates
            Console.Write("Enter allocation start date (yyyy-MM-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out var startDate)) { Console.WriteLine("Invalid start date"); return; }
            Console.Write("Enter allocation end date (yyyy-MM-dd) or leave empty for same day: ");
            var endInput = Console.ReadLine();
            DateTime endDate;
            if (string.IsNullOrWhiteSpace(endInput)) endDate = startDate;
            else if (!DateTime.TryParse(endInput, out endDate)) { Console.WriteLine("Invalid end date"); return; }

            // ensure within project dates
            if (startDate.Date < project.StartDate.Date || endDate.Date > project.EndDate.Date)
            {
                Console.WriteLine($"Allocation must fall within project dates ({project.StartDate:d} - {project.EndDate:d}). Aborting.");
                return;
            }

            const decimal workHoursPerDay = 8m;
            // show existing allocation per day
            Console.WriteLine("Existing allocation per day for selected employee:");
            var dayList = new List<DateTime>();
            for (var d = startDate.Date; d <= endDate.Date; d = d.AddDays(1))
            {
                dayList.Add(d);
                decimal existingPercent = 0m;
                decimal existingHours = 0m;
                foreach (var a in employee.Allocations ?? new List<TechNova.RMS.Domain.Models.Allocation>())
                {
                    if (a.AllocationPercent > 0)
                    {
                        var aStart = a.StartDate != default ? a.StartDate.Date : (a.Date?.Date ?? DateTime.MinValue);
                        var aEnd = a.EndDate != default ? a.EndDate.Date : (a.Date?.Date ?? DateTime.MinValue);
                        if (d >= aStart && d <= aEnd) existingPercent += a.AllocationPercent;
                    }
                    else if (a.HoursAllocated > 0 && a.Date.HasValue && a.Date.Value.Date == d)
                    {
                        existingHours += a.HoursAllocated;
                        existingPercent += Math.Min(100m, (a.HoursAllocated / workHoursPerDay) * 100m);
                    }
                }
                existingPercent = Math.Min(100m, existingPercent);
                if (existingHours == 0 && existingPercent > 0) existingHours = Math.Round((existingPercent / 100m) * workHoursPerDay, 2);
                var availPercent = Math.Max(0m, 100m - existingPercent);
                var availHours = Math.Round((availPercent / 100m) * workHoursPerDay, 2);
                Console.WriteLine($" - {d:d}: {existingPercent:F1}% ({existingHours:F1}h) available {availPercent:F1}% ({availHours:F1}h)");
            }

            Console.Write("Enter allocation percentage (0-100): ");
            if (!decimal.TryParse(Console.ReadLine(), out var percent) || percent <= 0 || percent > 100) { Console.WriteLine("Invalid percentage"); return; }

            // attempt allocation via service (enforces business rules)
            var allocDto = new AllocationDto
            {
                EmployeeId = Guid.TryParse(employee.Id, out var g) ? g : Guid.Empty,
                ProjectId = project.Id,
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                AllocationPercent = percent
            };

            var result = await _allocationService.AllocateAsync(allocDto);
            if (result == null || result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"Allocation failed: {result?.Message}");
            }
            else
            {
                Console.WriteLine("Allocation created successfully.");
            }
        }

        private async Task ShowAllocationReportAsync()
        {
            Console.Write("Report start date (yyyy-MM-dd): "); if (!DateTime.TryParse(Console.ReadLine(), out var start)) { Console.WriteLine("Invalid date"); return; }
            Console.Write("Report end date (yyyy-MM-dd): "); if (!DateTime.TryParse(Console.ReadLine(), out var end)) { Console.WriteLine("Invalid date"); return; }
            if (end < start) { Console.WriteLine("End must be >= start"); return; }

            const decimal workHoursPerDay = 8m;
            Console.WriteLine($"Allocations by date for {start:d} - {end:d}:");

            var anyDatePrinted = false;
            for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
            {
                var entries = new List<(string EmployeeName, string Department, string ProjectName, decimal Percent, decimal Hours)>();

                foreach (var emp in _db.Employees)
                {
                    if (emp.Allocations == null) continue;
                    foreach (var a in emp.Allocations)
                    {
                        // determine allocation effective start/end
                        var aStart = a.StartDate != default ? a.StartDate.Date : (a.Date?.Date ?? DateTime.MinValue);
                        var aEnd = a.EndDate != default ? a.EndDate.Date : (a.Date?.Date ?? DateTime.MinValue);

                        if (aStart <= d && d <= aEnd)
                        {
                            decimal percent = 0m;
                            decimal hours = 0m;
                            if (a.AllocationPercent > 0)
                            {
                                percent = a.AllocationPercent;
                                hours = Math.Round((percent / 100m) * workHoursPerDay, 2);
                            }
                            else if (a.HoursAllocated > 0)
                            {
                                hours = a.HoursAllocated;
                                percent = Math.Min(100m, (hours / workHoursPerDay) * 100m);
                            }
                            else
                            {
                                // nothing to report for this allocation
                                continue;
                            }

                            var proj = _db.Projects.FirstOrDefault(p => p.Id == a.ProjectId);
                            var projName = proj?.Name ?? a.ProjectId ?? "(unknown)";
                            entries.Add((emp.Name, emp.Department, projName, percent, hours));
                        }
                    }
                }

                if (!entries.Any())
                {
                    // skip dates with no allocations
                    continue;
                }

                anyDatePrinted = true;
                Console.WriteLine();
                Console.WriteLine($"Date: {d:d}");
                foreach (var e in entries.OrderByDescending(x => x.Percent).ThenBy(x => x.EmployeeName))
                {
                    Console.WriteLine($" - {e.EmployeeName} Dept={e.Department} Project={e.ProjectName} {e.Percent:F1}% ({e.Hours:F2}h)");
                }
            }

            if (!anyDatePrinted)
            {
                Console.WriteLine("No allocations in the selected date range.");
            }
        }

        private static bool RangesOverlap(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        {
            return aStart <= bEnd && bStart <= aEnd;
        }
    }
}

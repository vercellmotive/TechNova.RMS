using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using TechNova.RMS.Infrastructure.Data;
using TechNova.RMS.Infrastructure.Repositories;
using TechNova.RMS.Application.Interfaces.Repositories;
using TechNova.RMS.Application.Interfaces.Services;
using TechNova.RMS.Application.Services;

namespace TechNova.RMS.Infrastructure
{
    public class DependencyInjections
    {
        public static void RegisterServices(IServiceCollection services)
        {
            // Register application DB context as a singleton to keep in-memory state for the application's lifetime.
            // Swap this registration to an EF-backed implementation when you move to EntityFramework.
            services.AddSingleton<IAppDbContext, JsonDbContext>();

            // Register repositories and application services
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IAllocationRepository, AllocationRepository>();
            services.AddScoped<IAllocationService, AllocationService>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
        }
    }
}

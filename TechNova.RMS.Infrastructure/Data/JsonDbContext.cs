using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TechNova.RMS.Domain.Entities;
using TechNova.RMS.Domain.Models;

namespace TechNova.RMS.Infrastructure.Data
{
    /// <summary>
    /// A simple DB-context-like component that loads employee data from a JSON file once at startup
    /// and exposes an in-memory collection for read/write operations. Any changes are persisted
    /// back to the JSON file. This avoids repeated file reads for every query and simulates
    /// a lightweight DbContext backed by a file store.
    /// Reason: allows switching to a real DbContext later while keeping repository/service code unchanged.
    /// </summary>
    public class JsonDbContext : IAppDbContext
    {
        private readonly string[] JsonFilePaths = new[] { "employees.json", "projects.json" };

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        // In-memory cache of entities loaded at startup
        public System.Collections.ObjectModel.ObservableCollection<Employee> Employees { get; } = [];
        public System.Collections.ObjectModel.ObservableCollection<Project> Projects { get; } = [];

        private bool _employeesDirty;
        private bool _projectsDirty;
        private bool _suspendChangeTracking;

        public JsonDbContext()
        {
            _suspendChangeTracking = true;
            foreach (var file in JsonFilePaths)
            {
                var fullPath = Path.Combine(AppContext.BaseDirectory, "Data", file);
                var dir = Path.GetDirectoryName(fullPath) ?? AppContext.BaseDirectory;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                if (!File.Exists(fullPath)) File.WriteAllText(fullPath, "[]");

                try
                {
                    var text = File.ReadAllText(fullPath);

                    switch (file)
                    {
                        case "employees.json":
                            var employeeList = JsonSerializer.Deserialize<List<Employee>>(text, _jsonOptions);
                            Employees.Clear();
                            foreach (var e in employeeList ?? []) Employees.Add(e);
                            break;
                        case "projects.json":
                            var projectList = JsonSerializer.Deserialize<List<Project>>(text, _jsonOptions);
                            Projects.Clear();
                            foreach (var p in projectList ?? []) Projects.Add(p);
                            break;
                        default:
                            break;
                    }
                }
                catch
                {
                    // If deserialization fails for any file, leave the corresponding list empty.
                }
            }
            _suspendChangeTracking = false;

            // subscribe to collection changes to track dirty state
            RegisterCollection(Employees, () => _employeesDirty = true);
            RegisterCollection(Projects, () => _projectsDirty = true);
        }

        /// <summary>
        /// Persist current in-memory lists back to their respective JSON files.
        /// Call this method whenever you need to sync the in-memory state with disk.
        /// </summary>
        public void SaveChanges()
        {
            if (_employeesDirty) { SaveList(Employees, "employees.json"); _employeesDirty = false; }
            if (_projectsDirty) { SaveList(Projects, "projects.json"); _projectsDirty = false; }
        }

        private void RegisterCollection<T>(System.Collections.ObjectModel.ObservableCollection<T> collection, Action markDirty)
        {
            collection.CollectionChanged += (s, e) =>
            {
                if (_suspendChangeTracking) return;
                // any add/remove/replace/reset -> mark dirty
                markDirty();

                // attach/detach PropertyChanged handlers for items that implement INotifyPropertyChanged
                if (e.NewItems != null)
                {
                    foreach (var ni in e.NewItems)
                    {
                        if (ni is System.ComponentModel.INotifyPropertyChanged npc)
                        {
                            npc.PropertyChanged += (o, ev) => { if (!_suspendChangeTracking) markDirty(); };
                        }
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (var oi in e.OldItems)
                    {
                        if (oi is System.ComponentModel.INotifyPropertyChanged npc)
                        {
                            // best-effort: try to remove handler; since we used anonymous lambda above we can't remove it easily
                            // skipping explicit detach as items are typically short-lived in this simple context
                        }
                    }
                }
            };
        }

        private void SaveList<T>(IEnumerable<T> list, string fileName)
        {
            var fullPath = Path.Combine(AppContext.BaseDirectory, "Data", fileName);
            var dir = Path.GetDirectoryName(fullPath) ?? AppContext.BaseDirectory;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(list, _jsonOptions);
            File.WriteAllText(fullPath, json);
        }
    }
}

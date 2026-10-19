using System.Collections.ObjectModel;
using TechNova.RMS.Domain.Entities;

namespace TechNova.RMS.Infrastructure.Data
{
    /// <summary>
    /// Application-level abstraction for a data context.
    /// Implementations can be backed by JSON files, EF DbContext, or any other store.
    /// Repositories depend on this interface so swapping the concrete implementation
    /// requires only updating the DI registration.
    /// </summary>
    public interface IAppDbContext
    {
        ObservableCollection<Employee> Employees { get; }
        ObservableCollection<Project> Projects { get; }
        void SaveChanges();
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using TechNova.RMS.Domain.Entities;
using TechNova.RMS.Infrastructure.Data;

namespace TechNova.RMS.Infrastructure.Repositories
{
    public class BaseRepository
    {
        protected readonly TechNova.RMS.Infrastructure.Data.IAppDbContext _db;

        public BaseRepository(TechNova.RMS.Infrastructure.Data.IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
    }
}

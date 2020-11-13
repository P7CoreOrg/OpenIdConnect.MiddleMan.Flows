using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.EntityFrameworkCore
{
    public interface IConfigurationEntityCoreContext
    {
        DbSet<ExternalServiceEntity> ExternalServices { get; set; }

        DbContext DbContext { get; }
        Task<int> SaveChangesAsync();
    }
}

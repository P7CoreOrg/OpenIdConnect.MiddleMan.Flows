using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.EntityFrameworkCore
{
    public class ConfigurationEntityCoreContext : DbContext, IConfigurationEntityCoreContext
    {

        public ConfigurationEntityCoreContext(DbContextOptions<ConfigurationEntityCoreContext> options) : base(options) { }
        public DbSet<ExternalServiceEntity> ExternalServices { get; set; }

        public DbContext DbContext => this;

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }
    }
}

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
        DbSet<DownstreamOIDCConfigurationEntity> DownstreamOIDCConfigurations { get; set; }
        DbSet<OIDCClientConfigurationEntity> OIDCClientConfigurations { get; set; }
        DbSet<RedirectUriEntity> RedirectUris { get; set; }

        DbContext DbContext { get; }
        Task<int> SaveChangesAsync();
    }
}

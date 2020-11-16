using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace OIDCConsentOrchestrator.EntityFrameworkCore
{
    public class PostgresDbContextOptionsProvider : IDbContextOptionsProvider
    {
        private EntityFrameworkConnectionOptions _options;
        public PostgresDbContextOptionsProvider(IOptions<EntityFrameworkConnectionOptions> options)
        {
            _options = options.Value;
        }

        public void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _options.ConnectionString;
            optionsBuilder.UseNpgsql(connectionString);
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace OIDCConsentOrchestrator.EntityFrameworkCore
{
    public class MSSqlDbContextOptionsProvider : IDbContextOptionsProvider
    {
        private EntityFrameworkConnectionOptions _options;
        public MSSqlDbContextOptionsProvider(IOptions<EntityFrameworkConnectionOptions> options)
        {
            _options = options.Value;
        }

        public void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _options.ConnectionString;
            optionsBuilder.UseSqlServer(connectionString);
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}

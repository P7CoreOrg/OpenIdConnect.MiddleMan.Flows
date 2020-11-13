using Microsoft.EntityFrameworkCore;

namespace OIDCConsentOrchestrator.EntityFrameworkCore
{
    public interface IDbContextOptionsProvider
    {
        void Configure(DbContextOptionsBuilder optionsBuilder);
    }
}

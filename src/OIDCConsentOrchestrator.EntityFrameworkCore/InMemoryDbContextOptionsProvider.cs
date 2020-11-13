using Microsoft.EntityFrameworkCore;
using System;

namespace OIDCConsentOrchestrator.EntityFrameworkCore
{
    public class InMemoryDbContextOptionsProvider : IDbContextOptionsProvider
    {
        public void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            optionsBuilder.UseLazyLoadingProxies();
        }

        
    }
}

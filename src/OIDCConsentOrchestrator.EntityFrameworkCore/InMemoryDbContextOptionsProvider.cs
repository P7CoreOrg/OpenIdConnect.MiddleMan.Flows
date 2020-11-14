using Microsoft.EntityFrameworkCore;
using System;

namespace OIDCConsentOrchestrator.EntityFrameworkCore
{
    public class InMemoryDbContextOptionsProvider : IDbContextOptionsProvider
    {
        string GuidS => Guid.NewGuid().ToString();

        private string DatabaseName { get; }

        public InMemoryDbContextOptionsProvider()
        {
            DatabaseName = GuidS;
        }
         
        public void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(DatabaseName);
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}

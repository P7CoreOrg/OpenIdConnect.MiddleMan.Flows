using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.EntityFrameworkCore
{
    public class CosmosDbContextOptionsProvider : IDbContextOptionsProvider
    {
        private CosmosDbConfiguration _options;

        public CosmosDbContextOptionsProvider(IOptions<CosmosDbConfiguration> options)
        {
            _options = options.Value;
        }

        public void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseCosmos(_options.EndPointUrl, _options.PrimaryKey, _options.DatabaseName);
            optionsBuilder.UseLazyLoadingProxies();
        }


    }
}

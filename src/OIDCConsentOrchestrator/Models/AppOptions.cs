using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.Models
{
    public class AppOptions
    {
        public enum DatabaseTypes
        {
            Postgres,
            CosmosDB,
            InMemory
        }
        public DatabaseTypes DatabaseType { get; set; }
        public double CookieTTL { get; set; }
        public string DownstreamAuthority { get; set; }
    }
}

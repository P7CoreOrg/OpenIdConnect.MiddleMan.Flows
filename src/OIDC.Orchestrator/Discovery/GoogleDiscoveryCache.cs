
using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OIDC.Orchestrator.Discovery
{

    public class GoogleDiscoveryCache : DiscoveryCache, IGoogleDiscoveryCache
    {
        public GoogleDiscoveryCache(string authority, DiscoveryPolicy policy = null) : base(authority, policy)
        {
        }
        public GoogleDiscoveryCache(string authority, Func<HttpClient> httpClientFunc, DiscoveryPolicy policy = null) : base(authority, httpClientFunc, policy)
        {
        }
    }
}

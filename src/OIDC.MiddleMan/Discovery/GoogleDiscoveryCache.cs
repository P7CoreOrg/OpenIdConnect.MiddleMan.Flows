using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OIDC.ReferenceWebClient.Discovery
{

    public class GoogleDiscoveryCache : DiscoveryCache, IGoogleDiscoveryCache
    {
        public GoogleDiscoveryCache(DiscoveryClient client) : base(client)
        {
        }

        public GoogleDiscoveryCache(string authority, HttpClient client = null, DiscoveryPolicy policy = null) : base(authority, client, policy)
        {
        }

        public GoogleDiscoveryCache(string authority, Func<HttpClient> httpClientFunc, DiscoveryPolicy policy = null) : base(authority, httpClientFunc, policy)
        {
        }
    }
}

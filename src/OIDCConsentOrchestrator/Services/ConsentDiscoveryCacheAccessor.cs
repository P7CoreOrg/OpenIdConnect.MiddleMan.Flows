using OIDCConsentOrchestrator.EntityFrameworkCore;
using OIDCConsentOrchestrator.Models.Client;
using System.Collections.Concurrent;
using System.Net.Http;

namespace OIDCConsentOrchestrator.Services
{
    public class ConsentDiscoveryCacheAccessor : IConsentDiscoveryCacheAccessor
    {
        private IHttpClientFactory _httpClientFactory;
        ConcurrentDictionary<string, IConsentDiscoveryCache> _map;
        public ConsentDiscoveryCacheAccessor(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _map = new ConcurrentDictionary<string, IConsentDiscoveryCache>();
        }

        public IConsentDiscoveryCache GetConsentDiscoveryCache(ExternalServiceEntity externalServiceEntity)
        {
            IConsentDiscoveryCache value = null;
            if(!_map.TryGetValue(externalServiceEntity.Id,out value)){
                
                value = new ConsentDiscoveryCache(externalServiceEntity.Authority, () => _httpClientFactory.CreateClient());
                _map.TryAdd(externalServiceEntity.Id, value);
            }
            return value;
        }
    }
}

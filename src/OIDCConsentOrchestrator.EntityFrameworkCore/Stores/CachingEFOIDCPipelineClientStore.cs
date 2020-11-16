 
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using OIDCPipeline.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.EntityFrameworkCore.Stores
{
    class CachingEFOIDCPipelineClientStore<T> : IOIDCPipelineClientStore
        where T : IOIDCPipelineClientStore
    {
        private readonly IOIDCPipelineClientStore _inner;
        private readonly ICache<ClientRecord> _cacheClientRecord;
        private readonly ICache<List<string>> _cacheClientRedirectUris;
        private readonly ICache<List<string>> _cacheAllowedProtocolParamaters;
        private readonly ICache<string> _cacheClientSecret;
        private readonly ILogger _logger;
        static TimeSpan CachingExpiration { get; set; } = TimeSpan.FromHours(12);
        public CachingEFOIDCPipelineClientStore(
              T inner,
              ICache<ClientRecord> cacheClientRecord,
              ICache<List<string>> cacheClientRedirectUris,
              ICache<List<string>> cacheAllowedProtocolParamaters,
              ICache<string> cacheClientSecret,
              ILogger<CachingEFOIDCPipelineClientStore<T>> logger)
        {
            _inner = inner;
            _cacheClientRecord = cacheClientRecord;
            _cacheClientRedirectUris = cacheClientRedirectUris;
            _cacheAllowedProtocolParamaters = cacheAllowedProtocolParamaters;
            _cacheClientSecret = cacheClientSecret;
            _logger = logger;
        }
        public async Task<List<string>> FetchAllowedProtocolParamatersAsync(string scheme)
        {
            var result = await _cacheAllowedProtocolParamaters.GetAsync(scheme,
            CachingExpiration,
            () => _inner.FetchAllowedProtocolParamatersAsync(scheme),
            _logger);
            return result;
        }

        public async Task<ClientRecord> FetchClientRecordAsync(string scheme, string clientId)
        {
            var key = $"{scheme}.{clientId}";
            var result = await _cacheClientRecord.GetAsync(key,
               CachingExpiration,
               () => _inner.FetchClientRecordAsync(scheme,clientId),
               _logger);
            return result;
        }
 
    }
}

using IdentityServer4.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OIDCPipeline.Core.Configuration;
using System;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    internal class MemoryCacheAuthorizationCodeStore : IOIDCPipelineAuthorizationCodeStore
    {
        private IMemoryCache _memoryCache;
        private MemoryCacheOIDCPipelineStoreOptions _options;

        public MemoryCacheAuthorizationCodeStore(
            IOptions<MemoryCacheOIDCPipelineStoreOptions> options,
            IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _options = options.Value;
        }
        public Task<AuthorizationCode> GetAuthorizationCodeAsync(string code)
        {
            var result = _memoryCache.Get<AuthorizationCode>(code);
            return Task.FromResult(result);
        }

        public Task RemoveAuthorizationCodeAsync(string code)
        {
            _memoryCache.Remove(code);
            return Task.CompletedTask;
        }

        public Task<string> StoreAuthorizationCodeAsync(AuthorizationCode code)
        {
            var id = Guid.NewGuid().ToString("N");
            _memoryCache.Set(id, code, TimeSpan.FromMinutes(_options.ExpirationMinutes));
            return Task.FromResult(id);
        }
    }
}

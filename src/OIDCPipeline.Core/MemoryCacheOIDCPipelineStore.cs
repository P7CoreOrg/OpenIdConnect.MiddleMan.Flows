using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OIDC.ReferenceWebClient.Middleware;
using OIDCPipeline.Core.Configuration;
using System;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public class MemoryCacheOIDCPipelineStore : IOIDCPipelineStore
    {
        private IHttpContextAccessor _httpContextAccessor;
        private IMemoryCache _memoryCache;
        private MemoryCacheOIDCPipelineStoreOptions _options;

        public MemoryCacheOIDCPipelineStore(
            IHttpContextAccessor httpContextAccessor,
            IOptions<MemoryCacheOIDCPipelineStoreOptions> options,
            IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            _options = options.Value;
        }

        public Task DeleteStoredCacheAsync()
        {
            string id = _httpContextAccessor.HttpContext.Items[CookieTracerMiddlewareConstants.TracerName] as string;
            _memoryCache.Remove(OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id));
            _memoryCache.Remove(OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id));
            return Task.CompletedTask;
        }

        public Task<IdTokenResponse> GetDownstreamIdTokenResponse()
        {
            string id = _httpContextAccessor.HttpContext.Items[CookieTracerMiddlewareConstants.TracerName] as string;
            var result = _memoryCache.Get<IdTokenResponse>(OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id));
            return Task.FromResult(result);
        }

        public Task<IdTokenAuthorizationRequest> GetOriginalIdTokenRequestAsync()
        {
            string id = _httpContextAccessor.HttpContext.Items[CookieTracerMiddlewareConstants.TracerName] as string;
            var result = _memoryCache.Get<IdTokenAuthorizationRequest>(OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id));
            return Task.FromResult(result);
        }

        public Task StoreDownstreamIdTokenResponse(IdTokenResponse response)
        {
            var id = _httpContextAccessor.HttpContext.Items[CookieTracerMiddlewareConstants.TracerName] as string;
            _memoryCache.Set(OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id), response, TimeSpan.FromMinutes(_options.ExpirationMinutes));
            return Task.CompletedTask;
        }

        public Task StoreOriginalIdTokenRequestAsync(IdTokenAuthorizationRequest request)
        {
            var id = _httpContextAccessor.HttpContext.Items[CookieTracerMiddlewareConstants.TracerName] as string;
            _memoryCache.Set(OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id), request, TimeSpan.FromMinutes(_options.ExpirationMinutes));
            return Task.CompletedTask;
        }
    }
}

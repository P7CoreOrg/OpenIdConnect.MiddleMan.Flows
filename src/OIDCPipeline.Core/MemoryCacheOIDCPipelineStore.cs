using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OIDCPipeline.Core.Configuration;
using OIDCPipeline.Core.Validation.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    internal class MemoryCacheOIDCPipelineStore : IOIDCPipelineStore
    {
        private IMemoryCache _memoryCache;
        private MemoryCacheOIDCPipelineStoreOptions _options;

        public MemoryCacheOIDCPipelineStore(
            IOptions<MemoryCacheOIDCPipelineStoreOptions> options,
            IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _options = options.Value;
        }

        public Task DeleteStoredCacheAsync(string id)
        {
            _memoryCache.Remove(OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id));
            _memoryCache.Remove(OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id));
            return Task.CompletedTask;
        }

        public Task<IdTokenResponse> GetDownstreamIdTokenResponseAsync(string id)
        {
            var result = _memoryCache.Get<IdTokenResponse>(
                OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id));
            return Task.FromResult(result);
        }

        public Task<ValidatedAuthorizeRequest> GetOriginalIdTokenRequestAsync(string id)
        {
            var result = _memoryCache.Get<ValidatedAuthorizeRequest>(
                OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id));
            return Task.FromResult(result);
        }

        public Task StoreDownstreamCustomDataAsync(string id, Dictionary<string, object> custom)
        {
            var result = _memoryCache.Get<IdTokenResponse>(
                   OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id));
            if(result == null)
            {
                throw new Exception("Does not exist");
            }
            result.Custom = custom;
            return StoreDownstreamIdTokenResponseAsync(id, result);
        }

        public Task StoreDownstreamIdTokenResponseAsync(string id, IdTokenResponse response)
        {
            _memoryCache.Set(
                OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id), 
                response, 
                TimeSpan.FromMinutes(_options.ExpirationMinutes));
            return Task.CompletedTask;
        }

        public Task StoreOriginalIdTokenRequestAsync(string id, ValidatedAuthorizeRequest request)
        {
            _memoryCache.Set(
                OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id), 
                request, 
                TimeSpan.FromMinutes(_options.ExpirationMinutes));
            return Task.CompletedTask;
        }
    }
}

using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OIDCPipeline.Core.Configuration;
using OIDCPipeline.Core.Endpoints.ResponseHandling;
using OIDCPipeline.Core.Validation.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    internal class DistributedCacheOIDCPipelineStore : IOIDCPipelineStore
    {
        private IBinarySerializer _binarySerializer;
        private IDistributedCache _cache;
        private MemoryCacheOIDCPipelineStoreOptions _options;

        public DistributedCacheOIDCPipelineStore(
            IOptions<MemoryCacheOIDCPipelineStoreOptions> options,
            IBinarySerializer binarySerializer,
            IDistributedCache cache)
        {
            _binarySerializer = binarySerializer;
            _cache = cache;
            _options = options.Value;
        }

        public async Task DeleteStoredCacheAsync(string id)
        {
            var keyOriginal = OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id);
            var keyDownstream = OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id);
            await _cache.RemoveAsync(keyOriginal);
            await _cache.RemoveAsync(keyDownstream);

        }

        public async Task<DownstreamAuthorizeResponse> GetDownstreamIdTokenResponseAsync(string id)
        {
            var key = OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id);
            var result = await _cache.GetAsync(key);
            return _binarySerializer.Deserialize<DownstreamAuthorizeResponse>(result);
        }

        public async Task<ValidatedAuthorizeRequest> GetOriginalIdTokenRequestAsync(string id)
        {
            var key = OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id);
            var result = await _cache.GetAsync(key);
            return _binarySerializer.Deserialize<ValidatedAuthorizeRequest>(result);
        }

        public async Task StoreDownstreamCustomDataAsync(string id, Dictionary<string, object> custom)
        {
            var key = OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id);
            var result = await _cache.GetAsync(key);
            var value = _binarySerializer.Deserialize<DownstreamAuthorizeResponse>(result);

            if (value == null)
            {
                throw new Exception("Does not exist");
            }
            value.Custom = custom;
            await StoreDownstreamIdTokenResponseAsync(id, value);
        }

        public async Task StoreDownstreamIdTokenResponseAsync(string id, DownstreamAuthorizeResponse response)
        {
            var key = OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id);
            var data = _binarySerializer.Serialize(response);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.ExpirationMinutes)
            };
            await _cache.SetAsync(key, data, options);

        }

        public async Task StoreOriginalIdTokenRequestAsync(string id, ValidatedAuthorizeRequest request)
        {
            var key = OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id);

            var data = _binarySerializer.Serialize(request);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.ExpirationMinutes)
            };
            await _cache.SetAsync(key, data, options);
        }
    }
}

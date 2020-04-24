using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using OIDCPipeline.Core.Configuration;
using System;
using System.Net.Http;

namespace OIDC.ReferenceWebClient.Discovery
{
    public static class DownstreamDiscoveryCacheExtensions
    {
        public static IServiceCollection AddDownstreamDiscoveryCache(this IServiceCollection services)
        {

            services.AddSingleton<IDownstreamDiscoveryCache>(r =>
            {
                var factory = r.GetRequiredService<IHttpClientFactory>();
                var options = r.GetRequiredService<OIDCPipelineOptions>();
                return new DownstreamDiscoveryCache(options.DownstreamAuthority,
                    () => factory.CreateClient("downstream"),
                    new DiscoveryPolicy
                    {
                        ValidateEndpoints = false
                    });
            });
            return services;
        }
    }
}

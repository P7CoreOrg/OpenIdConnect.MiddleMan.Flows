using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace OIDC.ReferenceWebClient.Discovery
{
    public static class GoogleDiscoveryCacheExtensions
    {
        public static IServiceCollection AddGoogleDiscoveryCache(this IServiceCollection services)
        {
            services.AddSingleton<IGoogleDiscoveryCache>(r =>
            {
                var factory = r.GetRequiredService<IHttpClientFactory>();
                return new GoogleDiscoveryCache("https://accounts.google.com",() => factory.CreateClient());
            });
            return services;
        }
    }
}

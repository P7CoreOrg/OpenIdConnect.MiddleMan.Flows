using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IdentityModelExtras.Extensions
{
    public static class AspNetCoreServiceExtensions
    {
        public static IServiceCollection AddDefaultHttpClientFactory(this IServiceCollection services)
        {
            services.TryAddTransient<IDefaultHttpClientFactory, DefaultHttpClientFactory>();
            return services;
        }
    }
}

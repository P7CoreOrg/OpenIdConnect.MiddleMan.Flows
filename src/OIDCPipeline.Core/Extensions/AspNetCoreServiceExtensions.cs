using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OIDC.ReferenceWebClient.Discovery;
using OIDC.ReferenceWebClient.Middleware;
using OIDCPipeline.Core.AuthorizationEndpoint;
using OIDCPipeline.Core.Configuration;
using OIDCPipeline.Core.Endpoints;
using OIDCPipeline.Core.Hosting;
using static OIDCPipeline.Core.Constants;

namespace OIDCPipeline.Core.Extensions
{
    public static class AspNetCoreServiceExtensions
    {
        /// <summary>
        /// Adds the endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static IServiceCollection AddEndpoint<T>(this IServiceCollection services, string name, PathString path)
            where T : class, IEndpointHandler
        {
            services.AddTransient<T>();
            services.AddSingleton(new Hosting.Endpoint(name, path, typeof(T)));

            return services;
        }
        /// <summary>
        /// Adds the required platform services.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IServiceCollection AddRequiredPlatformServices(this IServiceCollection services)
        {

            services.AddOptions();
            services.AddSingleton(
                resolver => resolver.GetRequiredService<IOptions<OIDCPipelineOptions>>().Value);

            return services;
        }
        public static void AddOIDCPipeline(this IServiceCollection services, Action<OIDCPipelineOptions> setupAction)
        {
            services.AddRequiredPlatformServices();
            services.Configure(setupAction);
            services.AddTransient<IOIDCResponseGenerator, OIDCResponseGenerator>();
            services.AddTransient<IAuthorizeRequestValidator, AuthorizeRequestValidator>();
            services.AddDownstreamDiscoveryCache();
            services.AddEndpoint<DiscoveryEndpoint>(EndpointNames.Discovery, ProtocolRoutePaths.DiscoveryConfiguration.EnsureLeadingSlash());
            services.AddEndpoint<AuthorizeEndpoint>(EndpointNames.Authorize, ProtocolRoutePaths.Authorize.EnsureLeadingSlash());
            services.AddTransient<IEndpointRouter, EndpointRouter>();

        }
        public static void AddMemoryCacheOIDCPipelineStore(this IServiceCollection services, Action<MemoryCacheOIDCPipelineStoreOptions> setupAction)
        {
            services.Configure(setupAction);
            services.AddTransient<IOIDCPipelineStore, MemoryCacheOIDCPipelineStore>();
        }
       
        public static IApplicationBuilder UseOIDCPipelineStore(this IApplicationBuilder app)
        {
            app.UseMiddleware<CookieTracerMiddleware>();
            app.UseMiddleware<OIDCPipelineMiddleware>();

            return app;
        }
    }
}

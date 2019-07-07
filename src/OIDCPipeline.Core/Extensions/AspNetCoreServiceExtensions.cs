using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OIDC.ReferenceWebClient.Middleware;
using OIDCPipeline.Core.AuthorizationEndpoint;
using OIDCPipeline.Core.Configuration;

namespace OIDCPipeline.Core.Extensions
{
    public static class AspNetCoreServiceExtensions
    {
      
       
        public static void AddMemoryCacheOIDCPipelineStore(this IServiceCollection services, Action<MemoryCacheOIDCPipelineStoreOptions> setupAction)
        {
            services.Configure(setupAction);
            services.AddTransient<IOIDCPipelineStore, MemoryCacheOIDCPipelineStore>();
        }
        public static void AddOIDCPipeline(this IServiceCollection services)
        {
            services.AddTransient<IOIDCResponseGenerator, OIDCResponseGenerator>();
            services.AddTransient<IAuthorizeRequestValidator, AuthorizeRequestValidator>();
        }
        public static IApplicationBuilder UseOIDCPipelineStore(this IApplicationBuilder app)
        {
            app.UseMiddleware<CookieTracerMiddleware>();
            return app;
        }
    }
}

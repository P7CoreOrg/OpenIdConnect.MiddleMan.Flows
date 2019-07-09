using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OIDC.ReferenceWebClient.Data;
using OIDC.ReferenceWebClient.Discovery;
using OIDC.ReferenceWebClient.InMemoryIdentity;
using OIDCPipeline.Core;
using OIDCPipeline.Core.Extensions;
using OpenIdConntectModels;

namespace OIDC.ReferenceWebClient
{
 
    public static class Global
    {
        public static IServiceProvider ServiceProvider { get; set; }
    }
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var oAuth2SchemeRecords = new List<OAuth2SchemeRecord>();
            var section = Configuration.GetSection("oauth2");
            section.Bind(oAuth2SchemeRecords);

            section = Configuration.GetSection("clientSecrets");
            var clientSecrets = new Dictionary<string, string>();
            section.Bind(clientSecrets);

            section = Configuration.GetSection("oidcOptionStore");
            var oidcSchemeRecords = new Dictionary<string, OIDCSchemeRecord>();
            section.Bind(oidcSchemeRecords);
            services.AddTransient<IClientSecretStore>(sp =>
            {
                return new InMemoryClientSecretStore(oidcSchemeRecords);
            });
            services.AddHttpClient();
            services.AddGoogleDiscoveryCache();
            services.AddMemoryCacheOIDCPipelineStore(options =>
            {
                options.ExpirationMinutes = 30;
            });
            var downstreamAuthortityScheme = Configuration["downstreamAuthorityScheme"];

            var record = (from item in oAuth2SchemeRecords
                          where item.Scheme == downstreamAuthortityScheme
                          select item).FirstOrDefault();


            services.AddOIDCPipeline(options =>
            {
                //     options.DownstreamAuthority = "https://accounts.google.com";
                options.DownstreamAuthority = record.Authority;
            });
            services.AddTransient<ISigninManager, OIDCPipelineSigninManager>();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddInMemoryIdentity<ApplicationUser, ApplicationRole>().AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = $"{Configuration["applicationName"]}.AspNetCore.Identity.Application";
            });
            services.AddAuthentication<ApplicationUser>(Configuration);

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddSessionStateTempDataProvider();

            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.Cookie.Name = $"{Configuration["applicationName"]}.Session";
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromSeconds(3600);
                options.Cookie.HttpOnly = true;
            });

            Global.ServiceProvider = services.BuildServiceProvider();

            return Global.ServiceProvider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
           

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseOIDCPipelineStore();
            app.UseAuthentication();
            app.UseSession();
            app.UseMvc();

        }
    }
}
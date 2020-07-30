using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using OIDC.Orchestrator.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OIDCPipeline.Core;
using OIDC.Orchestrator.Discovery;
using OIDCPipeline.Core.Extensions;
using OIDC.Orchestrator.InMemoryIdentity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenIdConnectModels;
using Common;

namespace OIDC.Orchestrator
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        private ILogger _logger;
        private Exception _deferedException;
        readonly string MyAllowEverything = "CorsPolicy";
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _logger = new LoggerBuffered(LogLevel.Debug);
            _logger.LogInformation($"Create Startup {env.ApplicationName} - {env.EnvironmentName}");
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                _logger.LogInformation($"ConfigureServices");
                services.AddSingleton<IBinarySerializer, BinarySerializer>();
                services.AddSingleton<ISerializer, Serializer>();
                services.AddDistributedMemoryCache();
                services.AddCors(options =>
                {
                    options.AddPolicy(MyAllowEverything,
                        corsBuilder => corsBuilder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());
                });
                var openIdConnectSchemeRecordSchemeRecords = new List<OpenIdConnectSchemeRecord>();
                var section = Configuration.GetSection("openIdConnect");
                section.Bind(openIdConnectSchemeRecordSchemeRecords);


                section = Configuration.GetSection("oidcOptionStore");
                var oidcSchemeRecords = new Dictionary<string, OIDCSchemeRecord>();
                section.Bind(oidcSchemeRecords);
                services.AddTransient<IOIDCPipelineClientStore>(sp =>
                {
                    return new InMemoryClientSecretStore(oidcSchemeRecords);
                });
                services.AddHttpClient();
                services.AddGoogleDiscoveryCache();
                services.AddDistributedCacheOIDCPipelineStore(options =>
                {
                    options.ExpirationMinutes = 30;
                });
                /*
                services.AddMemoryCacheOIDCPipelineStore(options =>
                {
                    options.ExpirationMinutes = 30;
                });
                */
                var downstreamAuthortityScheme = Configuration["downstreamAuthorityScheme"];

                var record = (from item in openIdConnectSchemeRecordSchemeRecords
                              where item.Scheme == downstreamAuthortityScheme
                              select item).FirstOrDefault();

                services.AddOIDCPipeline(options =>
                {
                    //     options.DownstreamAuthority = "https://accounts.google.com";
                    options.DownstreamAuthority = record.Authority;
                });



                services.AddHttpClient("downstreamDocument", (a, b) =>
                {

                });
                services.AddHttpClient("github", c =>
                {

                    c.BaseAddress = new Uri("https://api.github.com/");
                    // Github API versioning
                    c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                    // Github requires a user-agent
                    c.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
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
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
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
            }
            catch (Exception ex)
            {
                // this will be thrown after we do some logging.
                _deferedException = ex;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IServiceProvider serviceProvider,
            ILogger<Startup> logger)
        {
            try
            {
                // Fist thing, copy our in-memory logs to the real ILogger
                //----------------------------------------------------------------------
                (_logger as LoggerBuffered).CopyToLogger(logger);

                // Second thing.  We are done with the in-memory stuff, so assign _logger to the real ILogger.
                //----------------------------------------------------------------------
                _logger = logger;

                // Third thing.  Take care of that Defered Exception
                //----------------------------------------------------------------------
                if (_deferedException != null)
                {
                    // defered throw.
                    throw _deferedException;
                }
                _logger.LogInformation("Configure");

                Global.ServiceProvider = serviceProvider;

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
                app.UseCors(MyAllowEverything);

                app.UseRouting();

                app.UseOIDCPipelineStore();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseSession();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                throw;
            }
        }
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OIDCConsentOrchestrator.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Identity.UI.Services;
using Common.Services;
using OIDCConsentOrchestrator.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIdConnectModels;
using OIDCConsentOrchestrator.EntityFrameworkCore.Extensions;
using OIDCConsentOrchestrator.EntityFrameworkCore.Services;
using Nito.AsyncEx;
using OIDCConsentOrchestrator.Models.Client;
using System.Net.Http;
using OIDCConsentOrchestrator.Services;
using OIDCPipeline.Core;
using OIDCConsentOrchestrator.Authentication;
using OIDCPipeline.Core.Extensions;
using OIDCConsentOrchestrator.EntityFrameworkCore;
using OIDCConsentOrchestrator.EntityFrameworkCore.Stores;
using OIDCPipeline.Core.Services;

namespace OIDCConsentOrchestrator
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostEnvironment HostingEnvironment { get; }
        public List<OpenIdConnectSchemeRecord> OpenIdConnectSchemeRecords { get; private set; }
        public Dictionary<string, OIDCSchemeRecord> OIDCOptionStore { get; private set; }
        public AppOptions AppOptions { get; private set; }

        private ILogger _logger;
        private Exception _deferedException;
        public Startup(
            IConfiguration configuration,
            IHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
            _logger = new LoggerBuffered(LogLevel.Debug);
            _logger.LogInformation($"Create Startup {hostingEnvironment.ApplicationName} - {hostingEnvironment.EnvironmentName}");
        }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                AppOptions = Configuration
                      .GetSection("AppOptions")
                      .Get<AppOptions>();

                services.Configure<DataProtectionOptions>(Configuration.GetSection("DataProtectionOptions"));

                OpenIdConnectSchemeRecords = Configuration
                  .GetSection("OpenIdConnect")
                  .Get<List<OpenIdConnectSchemeRecord>>();

                OIDCOptionStore = Configuration
                  .GetSection("oidcOptionStore")
                  .Get<Dictionary<string, OIDCSchemeRecord>>();

                services.AddSingleton<IOIDCPipelineClientStore>(sp =>
                {
                    return new InMemoryClientSecretStore(OIDCOptionStore);
                });
       


                services.AddHttpClient();
                services.AddSingleton<IConsentDiscoveryCacheAccessor, ConsentDiscoveryCacheAccessor>();
                 

                switch (AppOptions.DatabaseType)
                {
                    default:
                    case AppOptions.DatabaseTypes.InMemory:
                        services.AddInMemoryDbContextOptionsProvider();
                        break;
                    case AppOptions.DatabaseTypes.MSSQLLocalDB:
                    //    services.AddEntityFrameworkSqlServer();
                        services.AddMSSqlDbContextOptionsProvider();
                        break;
                    case AppOptions.DatabaseTypes.Postgres:
                    //    services.AddEntityFrameworkNpgsql();
                        services.AddPostgresDbContextOptionsProvider();
                        break;
                    case AppOptions.DatabaseTypes.CosmosDB:
                   //     services.AddEntityFrameworkCosmos();
                        services.AddCosmosDbContextOptionsProvider();
                        break;
                }
                services.AddDbContextOIDCConsentOrchestrator();
                services.Configure<OIDCConsentOrchestrator.EntityFrameworkCore.CosmosDbConfiguration>(Configuration.GetSection("CosmosDbConfiguration"));
                services.Configure<OIDCConsentOrchestrator.EntityFrameworkCore.EntityFrameworkConnectionOptions>(Configuration.GetSection("EntityFramworkConnectionOptions"));
                

                var openIdConnectSchemeRecordSchemeRecords = Configuration
                   .GetSection("openIdConnect")
                   .Get<List<OpenIdConnectSchemeRecord>>();

                services.AddSingleton<IOpenIdConnectSchemeRecords>(new InMemoryOpenIdConnectSchemeRecords(openIdConnectSchemeRecordSchemeRecords));

                services.AddDistributedMemoryCache();
                services.AddAuthentication();
                services.AddAuthentication<IdentityUser>(OpenIdConnectSchemeRecords);
                services.AddGoogleDiscoveryCache();

                //    services.AddAuthentication<IdentityUser>(Configuration);
                services.AddScoped<ISigninManager, DefaultSigninManager>();
                services.AddDbContext<ApplicationDbContext>(config =>
                {
                    // for in memory database  
                    config.UseInMemoryDatabase("InMemoryDataBase");
                });
                services.AddScoped<IEmailSender, NullEmailSender>();

                services.AddDatabaseDeveloperPageExceptionFilter();
                services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();
                /*
                 * services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                      .AddEntityFrameworkStores<ApplicationDbContext>();
                */

                services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, SeedSessionClaimsPrincipalFactory>();

                services.AddOIDCPipeline(options =>
                {
                    //     options.DownstreamAuthority = "https://accounts.google.com";
                    options.Scheme = AppOptions.DownstreamAuthorityScheme;
                });
                services.AddDistributedCacheOIDCPipelineStore(options =>
                {
                    options.ExpirationMinutes = 30;
                });
                services.AddSingleton<IBinarySerializer, BinarySerializer>();
                services.AddSingleton<ISerializer, Serializer>();

                services.AddControllers()
                                   .AddSessionStateTempDataProvider();
                IMvcBuilder builder = services.AddRazorPages();
                builder.AddSessionStateTempDataProvider();
                if (HostingEnvironment.IsDevelopment())
                {
                    builder.AddRazorRuntimeCompilation();
                }

                #region COOKIES
                //*************************************************
                //*********** COOKIE Start ************************
                //*************************************************


                services.Configure<CookiePolicyOptions>(options =>
                {
                    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });
                services.ConfigureExternalCookie(config =>
                {
                    config.Cookie.SameSite = SameSiteMode.None;
                });
                services.ConfigureApplicationCookie(options =>
                {
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                    options.ExpireTimeSpan = TimeSpan.FromSeconds(AppOptions.CookieTTL);
                    options.SlidingExpiration = true;
                 //   options.Cookie.Name = $"{Configuration["applicationName"]}.AspNetCore.Identity.Application";
                    options.LoginPath = $"/Identity/Account/Login";
                    options.LogoutPath = $"/Identity/Account/Logout";
                    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
                    options.Events = new CookieAuthenticationEvents()
                    {

                        OnRedirectToLogin = (ctx) =>
                        {
                            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == StatusCodes.Status200OK)
                            {
                                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return Task.CompletedTask;
                            }
                            ctx.Response.Redirect(ctx.RedirectUri);
                            return Task.CompletedTask;
                        },
                        OnRedirectToAccessDenied = (ctx) =>
                        {
                            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == StatusCodes.Status200OK)
                            {
                                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                                return Task.CompletedTask;
                            }
                            ctx.Response.Redirect(ctx.RedirectUri);
                            return Task.CompletedTask;
                        }
                    };
                });


                //*************************************************
                //*********** COOKIE END **************************
                //*************************************************
                #endregion
                #region SESSION
                services.AddSession(options =>
                {
                    // options.Cookie.Name = $"{Configuration["applicationName"]}.Session";
                    options.Cookie.IsEssential = true;
                    // Set a short timeout for easy testing.
                    options.IdleTimeout = TimeSpan.FromSeconds(AppOptions.CookieTTL);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });
                #endregion
            }
            catch (Exception ex)
            {
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
            if (_logger is LoggerBuffered)
            {
                (_logger as LoggerBuffered).CopyToLogger(logger);
            }
            _logger = logger;
            _logger.LogInformation("Configure");
            if (_deferedException != null)
            {
                _logger.LogError(_deferedException.Message);
                throw _deferedException;
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            if (HostingEnvironment.IsDevelopment())
            {
                var admin = serviceProvider.GetRequiredService<IOIDCConsentOrchestratorAdmin>();
                AsyncContext.Run(async () =>
                {
                    await admin.UpsertEntityAsync(new EntityFrameworkCore.ExternalServiceEntity
                    {
                        Description ="Sample External Service",
                        Name="Sample External Service",
                        Authority = "https://localhost:9001/api/Consent"

                    });
                    foreach(var item in OIDCOptionStore)
                    {
                        var entity = new DownstreamOIDCConfigurationEntity
                        {
                            Name = item.Key,
                            OIDCClientConfigurations = new List<OIDCClientConfigurationEntity>()
                        };
                        foreach(var clientRecord in item.Value.ClientRecords)
                        {
                            var oidcClientConfigurationEntity = new OIDCClientConfigurationEntity();
                            oidcClientConfigurationEntity.ClientId = clientRecord.Key;
                            oidcClientConfigurationEntity.ClientSecret = clientRecord.Value.Secret;
                            oidcClientConfigurationEntity.RedirectUris = new List<RedirectUriEntity>();
                            foreach(var ru in clientRecord.Value.RedirectUris)
                            {
                                oidcClientConfigurationEntity.RedirectUris.Add(new RedirectUriEntity
                                {
                                    RedirectUri = ru
                                }); 
                            }
                            entity.OIDCClientConfigurations.Add(oidcClientConfigurationEntity);
                        }

                        await admin.UpsertEntityAsync(entity);
                    }
                   
                });
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseOIDCPipeline();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<AuthSessionValidationMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}

using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using OIDCOrchestratorApp.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using OIDCOrchestratorApp.Services;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using OIDCPipeline.Core;
using Common;
using System.Collections.Generic;
using OpenIdConnectModels;

using OIDCPipeline.Core.Extensions;
using OIDCOrchestratorApp.Extensions;
using OIDCPipeline.Core.Services;
using FluffyBunny4.DotNetCore.Services;

namespace OIDCOrchestratorApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostEnvironment HostingEnvironment { get; }
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
                services.AddAuthentication();
                services.AddAuthentication<IdentityUser>(Configuration);
                services.AddHttpClient();
                services.AddGoogleDiscoveryCache();
                services.AddDistributedCacheOIDCPipelineStore(options =>
                {
                    options.ExpirationMinutes = 30;
                });
                var downstreamAuthortityScheme = Configuration["downstreamAuthorityScheme"];

                services.AddSingleton<IOpenIdConnectSchemeRecords>(new InMemoryOpenIdConnectSchemeRecords(openIdConnectSchemeRecordSchemeRecords));

                var record = (from item in openIdConnectSchemeRecordSchemeRecords
                              where item.Scheme == downstreamAuthortityScheme
                              select item).FirstOrDefault();

                services.AddOIDCPipeline(options =>
                {
                    //     options.DownstreamAuthority = "https://accounts.google.com";
                    options.Scheme = downstreamAuthortityScheme;
                });

                services.AddSingleton<IBinarySerializer, BinarySerializer>();
                services.AddSingleton<ISerializer, Serializer>();
                services.AddDistributedMemoryCache();
                services.AddScoped<ISigninManager, DefaultSigninManager>();
                services.AddDbContext<ApplicationDbContext>(config =>
                {
                    // for in memory database  
                    config.UseInMemoryDatabase("InMemoryDataBase");
                });
                services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();
                // services.AddDefaultIdentity must be adding its own fake
                // Switched to services.AddIdentity<IdentityUser,IdentityRole>, and now I have to add it.
                services.AddScoped<IEmailSender, FakeEmailSender>();

                services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, SeedSessionClaimsPrincipalFactory>();

                services.AddControllers()
                   .AddSessionStateTempDataProvider();
                IMvcBuilder builder = services.AddRazorPages();
                builder.AddSessionStateTempDataProvider();
                if (HostingEnvironment.IsDevelopment())
                {
                    builder.AddRazorRuntimeCompilation();
                }

                #region Cookies
                //*************************************************
                //*********** COOKIE Start ************************
                //*************************************************

                var cookieTTL = Convert.ToInt32(Configuration["authAndSessionCookies:ttl"]);
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

                    options.ExpireTimeSpan = TimeSpan.FromSeconds(cookieTTL);
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
                    options.IdleTimeout = TimeSpan.FromSeconds(cookieTTL);
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

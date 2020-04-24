using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OIDC.Orchestrator.InMemoryIdentity;
using OIDCPipeline.Core;
using OIDCPipeline.Core.Endpoints.ResponseHandling;
using OpenIdConnectModels;

namespace OIDC.Orchestrator.InMemoryIdentity
{
    public class SimpleOpenIdConnectProtocolValidator : OpenIdConnectProtocolValidator
    {
        public override string GenerateNonce()
        {
            return Guid.NewGuid().ToString();
     //       return base.GenerateNonce();
        }

    }
    public class MyOpenIdConnectProtocolValidator : OpenIdConnectProtocolValidator
    {
        private readonly IOIDCPipelineStore _oidcPipelineStore;
        private readonly IHttpContextAccessor _accessor;

        public MyOpenIdConnectProtocolValidator(IOIDCPipelineStore oidcPipelineStore, IHttpContextAccessor accessor)
        {
            _oidcPipelineStore = oidcPipelineStore;
            _accessor = accessor;
        }

        public override string GenerateNonce()
        {
            /*
            var sp = _sp;
            var oidcPipelineStore = sp.GetRequiredService<IOIDCPipelineStore>();
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            
*/
            var oidcPipelineStore = _oidcPipelineStore;
            var httpContextAccessor = _accessor;
            string nonce = httpContextAccessor.HttpContext.GetOIDCPipeLineKey();

            var original = oidcPipelineStore.GetOriginalIdTokenRequestAsync(nonce).GetAwaiter().GetResult();

            if (original != null)
            {

                if (!string.IsNullOrWhiteSpace(original.Nonce))
                {
                    return original.Nonce;
                }
            }

            nonce = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString() + Guid.NewGuid().ToString()));
            if (RequireTimeStampInNonce)
            {
                return DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture) + "." + nonce;
            }

            return nonce;
        }
    }

    public static class InMemoryIdentityServiceCollectionExtensions
    {
        public static IdentityBuilder AddAuthentication<TUser>(this IServiceCollection services, IConfiguration configuration)
            where TUser : class => services.AddAuthentication<TUser>(configuration, null);

        public static IdentityBuilder AddAuthentication<TUser>(this IServiceCollection services,
            IConfiguration configuration,
            Action<IdentityOptions> setupAction)
            where TUser : class
        {
            // Services used by identity
            
            var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            });
            // doesn't work here either.
            //----------------------------------
            /*
            services.AddOptions<OpenIdConnectOptions>()
                       .Configure<IOIDCPipelineStore, IHttpContextAccessor>((options, oidcPipelineStore, accessor) =>
                       {
                           options.ProtocolValidator = new MyOpenIdConnectProtocolValidator(oidcPipelineStore, accessor)
                           {
                               RequireTimeStampInNonce = false,
                               RequireStateValidation = false,
                               RequireNonce = true,
                               NonceLifetime = TimeSpan.FromMinutes(15)
                           };
                       });
                       */
            var section = configuration.GetSection("openIdConnect");
            var openIdConnectSchemeRecordSchemeRecords = new List<OpenIdConnectSchemeRecord>();
            section.Bind(openIdConnectSchemeRecordSchemeRecords);
            foreach (var record in openIdConnectSchemeRecordSchemeRecords)
            {
                var scheme = record.Scheme;
                services.AddOptions<OpenIdConnectOptions>(scheme)
                                     .Configure<IOIDCPipelineStore, IHttpContextAccessor>((options, oidcPipelineStore, accessor) =>
                                     {
                                         options.ProtocolValidator = new MyOpenIdConnectProtocolValidator(oidcPipelineStore, accessor)
                                         {
                                             RequireTimeStampInNonce = false,
                                             RequireStateValidation = false,
                                             RequireNonce = true,
                                             NonceLifetime = TimeSpan.FromMinutes(15)
                                         };
                                     });

               
                authenticationBuilder.AddOpenIdConnect(scheme, scheme, options =>
                {
                    options.Authority = record.Authority;
                    options.CallbackPath = record.CallbackPath;
                    options.RequireHttpsMetadata = false;
                    if (!string.IsNullOrEmpty(record.ResponseType))
                    {
                        options.ResponseType = record.ResponseType;
                    }
                    options.GetClaimsFromUserInfoEndpoint = record.GetClaimsFromUserInfoEndpoint;
                    options.ClientId = record.ClientId;
                    options.ClientSecret = record.ClientSecret;
                    options.SaveTokens = true;
                    options.TokenValidationParameters.ValidateAudience = false;
                    options.Events.OnMessageReceived = async context =>
                    {

                    };
                    options.Events.OnTokenValidated = async context =>
                    {
                        var pipeLineStore = context.HttpContext.RequestServices.GetRequiredService<IOIDCPipelineStore>();

                        OpenIdConnectMessage oidcMessage = null;
                        if (context.Options.ResponseType == "id_token")
                        {
                            oidcMessage = context.ProtocolMessage;
                        }
                        else
                        {
                            oidcMessage = context.TokenEndpointResponse;
                        }

                        var userState = context.ProtocolMessage.Parameters["state"].Split('.')[0];

                        var header = new JwtHeader();
                        var handler = new JwtSecurityTokenHandler();
                        var idToken = handler.ReadJwtToken(oidcMessage.IdToken);
                        var claims = idToken.Claims.ToList();

                        var stored = await pipeLineStore.GetOriginalIdTokenRequestAsync(userState);

                        DownstreamAuthorizeResponse idTokenResponse = new DownstreamAuthorizeResponse
                        {
                            Request = stored,
                            AccessToken = oidcMessage.AccessToken,
                            ExpiresAt = oidcMessage.ExpiresIn,
                            IdToken = oidcMessage.IdToken,
                            RefreshToken = oidcMessage.RefreshToken,
                            TokenType = oidcMessage.TokenType,
                            LoginProvider = scheme
                        };
                        await pipeLineStore.StoreDownstreamIdTokenResponseAsync(userState, idTokenResponse);

                    };
                    options.Events.OnRedirectToIdentityProvider = async context =>
                    {
                        string key = context.HttpContext.GetOIDCPipeLineKey();
                        var pipeLineStore = context.HttpContext.RequestServices.GetRequiredService<IOIDCPipelineStore>();
                        var stored = await pipeLineStore.GetOriginalIdTokenRequestAsync(key);
                        var clientSecretStore = context.HttpContext.RequestServices.GetRequiredService<IOIDCPipelineClientStore>();

                        if (stored != null)
                        {
                            context.ProtocolMessage.ClientId = stored.ClientId;
                            context.Options.ClientId = stored.ClientId;
                            context.Options.ClientSecret = await clientSecretStore.FetchClientSecretAsync(scheme,
                                stored.ClientId);
                            context.ProtocolMessage.State = $"{key}.";
                        }

                        context.Options.Authority = context.Options.Authority;
                        if (record.AdditionalProtocolScopes != null && record.AdditionalProtocolScopes.Any())
                        {
                            string additionalScopes = "";
                            foreach (var item in record.AdditionalProtocolScopes)
                            {
                                additionalScopes += $" {item}";
                            }
                            context.ProtocolMessage.Scope += additionalScopes;
                        }
                        if (context.HttpContext.User.Identity.IsAuthenticated)
                        {
                            // assuming a relogin trigger, so we will make the user relogin on the IDP
                            context.ProtocolMessage.Prompt = "login";
                        }
                        var allowedParams = await clientSecretStore.FetchAllowedProtocolParamatersAsync(scheme);

                        foreach (var allowedParam in allowedParams)
                        {
                            var item = stored.Raw[allowedParam];
                            if (item != null)
                            {
                                if (string.Compare(allowedParam, "state", true) == 0)
                                {
                                    context.ProtocolMessage.SetParameter(allowedParam, $"{key}.{item}");
                                }
                                else
                                {
                                    context.ProtocolMessage.SetParameter(allowedParam, item);
                                }
                            }
                        }
                        /*
                        if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                        {
                            context.ProtocolMessage.AcrValues = "v1=google";
                        }
                        */

                    };
                    options.Events.OnRemoteFailure = context =>
                    {
                        context.Response.Redirect("/");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    };
                });
            }


            return new IdentityBuilder(typeof(TUser), services);
        }
    }
}

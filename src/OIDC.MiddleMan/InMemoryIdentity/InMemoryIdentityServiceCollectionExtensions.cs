using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using OIDC.ReferenceWebClient.Extensions;
using OIDCPipeline.Core;
using OpenIdConntectModels;

namespace OIDC.ReferenceWebClient.InMemoryIdentity
{
    public class MyOpenIdConnectProtocolValidator : OpenIdConnectProtocolValidator
    {

        public override string GenerateNonce()
        {
            var sp = Global.ServiceProvider;
            var oidcPipelineStore = sp.GetRequiredService<IOIDCPipelineStore>();
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();

            string nonce = httpContextAccessor.HttpContext.GetStringCookie(".oidc.Nonce.Tracker");
            var original = oidcPipelineStore.GetOriginalIdTokenRequestAsync(nonce).GetAwaiter().GetResult();

            if (original != null)
            {
              
                if (!string.IsNullOrWhiteSpace(original.nonce))
                {
                    return original.nonce;
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

            var section = configuration.GetSection("oauth2");
            var oAuth2SchemeRecords = new List<OAuth2SchemeRecord>();
            section.Bind(oAuth2SchemeRecords);
            foreach (var record in oAuth2SchemeRecords)
            {
                var scheme = record.Scheme;
                authenticationBuilder.AddOpenIdConnect(scheme, scheme, options =>
                {
                    options.ProtocolValidator = new MyOpenIdConnectProtocolValidator()
                    {
                        RequireTimeStampInNonce = false,
                        RequireStateValidation = false,
                        RequireNonce = true,
                        NonceLifetime = TimeSpan.FromMinutes(15)
                    };
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
                    options.Events.OnTokenValidated = async context =>
                    {
                        var pipeLineStore = context.HttpContext.RequestServices.GetRequiredService<IOIDCPipelineStore>();
                       
                        OpenIdConnectMessage oidcMessage = null;
                        if(context.Options.ResponseType == "id_token")
                        {
                            oidcMessage = context.ProtocolMessage;
                        }
                        else
                        {
                            oidcMessage = context.TokenEndpointResponse;
                        }
                        var header = new JwtHeader();
                        var handler = new JwtSecurityTokenHandler();
                        var idToken = handler.ReadJwtToken(oidcMessage.IdToken);
                        var claims = idToken.Claims.ToList();
                        var nonce = (from item in claims where item.Type == OidcConstants.AuthorizeRequest.Nonce select item).FirstOrDefault();

                        IdTokenResponse idTokenResponse = new IdTokenResponse
                        {
                            access_token = oidcMessage.AccessToken,
                            expires_at = oidcMessage.ExpiresIn,
                            id_token = oidcMessage.IdToken,
                            refresh_token = oidcMessage.RefreshToken,
                            token_type = oidcMessage.TokenType,
                            LoginProvider = scheme
                        };
                        await pipeLineStore.StoreDownstreamIdTokenResponseAsync(nonce.Value,idTokenResponse);

                    };
                    options.Events.OnRedirectToIdentityProvider = async context =>
                    {
                        string nonce = context.HttpContext.GetStringCookie(".oidc.Nonce.Tracker");
                        var pipeLineStore = context.HttpContext.RequestServices.GetRequiredService<IOIDCPipelineStore>();
                        var stored = await pipeLineStore.GetOriginalIdTokenRequestAsync(nonce);
                        var clientSecretStore = context.HttpContext.RequestServices.GetRequiredService<IClientSecretStore>();

                        if (stored != null)
                        {
                            context.ProtocolMessage.ClientId = stored.client_id;
                            context.Options.ClientId = stored.client_id;
                            context.Options.ClientSecret = await clientSecretStore.FetchClientSecretAsync(scheme, stored.client_id);

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

                        foreach(var allowedParam in allowedParams)
                        {
                            var item = stored.ExtraValues[allowedParam];
                            if(item != null)
                            {
                                context.ProtocolMessage.SetParameter(allowedParam, item);
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

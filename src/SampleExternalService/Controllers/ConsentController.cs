using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OIDCConsentOrchestrator.Models;
using OIDCConsentOrchestrator.Models.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static OIDCConsentOrchestrator.Models.Constants;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SampleExternalService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsentController : ControllerBase
    {
        private IHttpContextAccessor _httpContextAccessor;
        private ILogger<ConsentController> _logger;

        public ConsentController(
            IHttpContextAccessor httpContextAccessor,
            ILogger<ConsentController> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [HttpGet(".well-known/consent-configuration")]
        public async Task<ConsentDiscoveryDocument> GetDiscoveryDocumentAsync()
        {
            return new ConsentDiscoveryDocument
            {
                AuthorizeEndpoint = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/api/consent/authorize",
                ScopesSupported = new List<string> 
                {
                    "sample",
                    "sample.read",
                    "sample.write"
                },
                AuthorizationType = AuthorizationTypes.SubjectAndScopes
            };
        }
        [HttpPost("authorize")]
        public async Task<AuthorizeResponse> PostAuthorizeAsync([FromBody] AuthorizeRequest authorizeRequest)
        {
            
            var authorizeResponse = new AuthorizeResponse
            {
                Authorized = false,
                Subject = authorizeRequest.Subject 
            };
            if (string.IsNullOrWhiteSpace(authorizeRequest.Subject)) 
            {
                authorizeResponse.Error = new Error
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "bad subject"
                };
                return authorizeResponse;
            }

            // we are a SubjectAndScopes controller so scopes have to be present;
            if (authorizeRequest.Scopes == null|| !authorizeRequest.Scopes.Any())
            {
                authorizeResponse.Error = new Error
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "No scopes where requested!"
                };
                return authorizeResponse;
            }

            // check if user is in our database.
            authorizeResponse.Authorized = authorizeRequest.Subject == "good";
            if (authorizeResponse.Authorized)
            {
                authorizeResponse.Scopes = authorizeRequest.Scopes;
            }
            else
            {
                authorizeResponse.Error = new Error
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "User is bad!"
                };
            }
            return authorizeResponse;
        }
    }
}

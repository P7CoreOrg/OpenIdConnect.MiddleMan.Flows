using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OIDCConsentOrchestrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
        public async Task<DiscoveryDocument> GetDiscoveryDocumentAsync()
        {
            return new DiscoveryDocument
            {
                AuthorizeEndpoint = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/api/consent/authorize",
                Scopes = new List<string> 
                {
                    "sample",
                    "sample.read",
                    "sample.write"
                },
                AuthorizeType = AuthorizeTypes.Passthrough
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
            // we are a passthrough controller so scopes have to be present;
            if (string.IsNullOrWhiteSpace(authorizeRequest.Subject)) 
            {
                authorizeResponse.Error = new Error
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "bad subject"
                };
                return authorizeResponse;
            }

            if(authorizeRequest.Scopes == null|| !authorizeRequest.Scopes.Any())
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

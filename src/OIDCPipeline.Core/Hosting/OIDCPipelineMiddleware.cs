using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OIDCPipeline.Core.AuthorizationEndpoint;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OIDCPipeline.Core.Hosting
{
    internal class OIDCPipelineMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public OIDCPipelineMiddleware(RequestDelegate next, ILogger<OIDCPipelineMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(
            HttpContext context,
            IEndpointRouter router)
        {
            try
            {
                var endpoint = router.Find(context);
                if (endpoint != null)
                {
                    _logger.LogInformation("Invoking IdentityServer endpoint: {endpointType} for {url}", endpoint.GetType().FullName, context.Request.Path.ToString());

                    var result = await endpoint.ProcessAsync(context);

                    if (result != null)
                    {
                        _logger.LogTrace("Invoking result: {type}", result.GetType().FullName);
                        await result.ExecuteAsync(context);
                    }

                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unhandled exception: {exception}", ex.Message);
                throw;
            }

            await _next(context);
        }
    }
}

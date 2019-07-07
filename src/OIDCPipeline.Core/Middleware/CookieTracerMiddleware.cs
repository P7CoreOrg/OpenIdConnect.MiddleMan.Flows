using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OIDC.ReferenceWebClient.Middleware
{
    internal static class CookieTracerMiddlewareConstants
    {
        public static string TracerName = ".oidcPipeline.TracerId";
    }
    internal class CookieTracerMiddleware
    {
        private readonly RequestDelegate _next;
        
        public CookieTracerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var tracerId = context.GetStringCookie(CookieTracerMiddlewareConstants.TracerName);
            if (string.IsNullOrWhiteSpace(tracerId))
            {
                tracerId = Guid.NewGuid().ToString("N");
            }
            context.Items[CookieTracerMiddlewareConstants.TracerName] = tracerId;
            // slide it out.
            context.SetStringCookie(CookieTracerMiddlewareConstants.TracerName, tracerId, 30);

            await _next(context);
        }
    }
}

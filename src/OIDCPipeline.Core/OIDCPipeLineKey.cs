using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIDCPipeline.Core
{
    public static class OIDCPipeLineKey
    {
        public static void SetOIDCPipeLineKey(this HttpContext context,string value)
        {
            context.SetStringCookie(".oidc.Nonce.Tracker", value, 60);
        }
        public static string GetOIDCPipeLineKey(this HttpContext context)
        {
            string key = context.GetStringCookie(".oidc.Nonce.Tracker");
            return key;
        }
    }
}


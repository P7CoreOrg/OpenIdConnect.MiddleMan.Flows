using System;
using System.Collections.Generic;
using System.Text;

namespace OIDCPipeline.Core
{
    internal class Constants
    {
        public static class EndpointNames
        {
            public const string Authorize = "Authorize";
            public const string Token = "Token";
            public const string Discovery = "Discovery";
        }
        public static class ProtocolRoutePaths
        {
            public const string ConnectPathPrefix = "connect";

            public const string Authorize = ConnectPathPrefix + "/authorize";
            
            public const string DiscoveryConfiguration = ".well-known/openid-configuration";
        
            public const string Token = ConnectPathPrefix + "/token";


            public static readonly string[] CorsPaths =
            {
                DiscoveryConfiguration,
              
                Token,
           
            };
        }
    }
}

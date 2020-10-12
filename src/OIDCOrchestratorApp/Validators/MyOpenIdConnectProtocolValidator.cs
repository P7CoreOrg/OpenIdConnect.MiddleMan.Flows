using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OIDCPipeline.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OIDCOrchestratorApp.Validators
{
    public class MyOpenIdConnectProtocolValidator : OpenIdConnectProtocolValidator
    {
        private string GuidN => Guid.NewGuid().ToString("N");

        private readonly IOIDCPipeLineKey _oidcPipeLineKey;
        private readonly IOIDCPipelineStore _oidcPipelineStore;

        public MyOpenIdConnectProtocolValidator(
            IOIDCPipeLineKey oidcPipeLineKey, 
            IOIDCPipelineStore oidcPipelineStore )
        {
            _oidcPipeLineKey = oidcPipeLineKey;
            _oidcPipelineStore = oidcPipelineStore;
        }

        public override string GenerateNonce()
        {
            /*
            var sp = _sp;
            var oidcPipelineStore = sp.GetRequiredService<IOIDCPipelineStore>();
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            
*/
            var oidcPipelineStore = _oidcPipelineStore;
            string nonce = _oidcPipeLineKey.GetOIDCPipeLineKey();

            var original = oidcPipelineStore.GetOriginalIdTokenRequestAsync(nonce).GetAwaiter().GetResult();

            if (original != null)
            {

                if (!string.IsNullOrWhiteSpace(original.Nonce))
                {
                    return original.Nonce;
                }
            }

            nonce = Convert.ToBase64String(Encoding.UTF8.GetBytes(GuidN + GuidN));
            if (RequireTimeStampInNonce)
            {
                return DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture) + "." + nonce;
            }

            return nonce;
        }
    }
}

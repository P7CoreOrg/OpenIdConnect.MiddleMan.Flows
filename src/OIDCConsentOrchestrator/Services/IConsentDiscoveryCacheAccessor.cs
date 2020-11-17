using OIDCConsentOrchestrator.EntityFrameworkCore;
using OIDCConsentOrchestrator.Models.Client;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OIDCConsentOrchestrator.Services
{

    public interface IConsentDiscoveryCacheAccessor
    {
        public IConsentDiscoveryCache GetConsentDiscoveryCache(ExternalServiceEntity externalServiceEntity);
    }
}

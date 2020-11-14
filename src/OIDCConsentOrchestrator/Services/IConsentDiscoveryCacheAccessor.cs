using OIDCConsentOrchestrator.EntityFrameworkCore;
using OIDCConsentOrchestrator.Models.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.Services
{
    public interface IConsentDiscoveryCacheAccessor
    {
        public IConsentDiscoveryCache GetConsentDiscoveryCache(ExternalServiceEntity externalServiceEntity);
    }
}

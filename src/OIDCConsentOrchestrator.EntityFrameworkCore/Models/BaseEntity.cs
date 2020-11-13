using OIDCConsentOrchestrator.Models;
using System;
using System.Collections.Generic;

namespace OIDCConsentOrchestrator.EntityFrameworkCore
{
    public class BaseEntity
    {
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }

}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OIDCConsentOrchestrator.EntityFrameworkCore
{
    public class OIDCClientConfigurationEntity: BaseEntity
    {
        public string DownstreamOIDCConfigurationFK { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        [ForeignKey("OIDCClientConfigurationFK")]
        public virtual ICollection<RedirectUriEntity> RedirectUris { get; set; }
 
    }
}

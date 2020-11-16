namespace OIDCConsentOrchestrator.EntityFrameworkCore
{
    public class RedirectUriEntity : BaseEntity
    {
        public string OIDCClientConfigurationFK { get; set; }
        public string RedirectUri { get; set; }
    }
}

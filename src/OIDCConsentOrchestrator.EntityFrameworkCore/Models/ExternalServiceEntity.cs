namespace OIDCConsentOrchestrator.EntityFrameworkCore
{
    public class ExternalServiceEntity : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Endpoint { get; set; }
    }

}

namespace OIDCConsentOrchestrator.EntityFrameworkCore
{

    //https://www.googleapis.com/auth/gmail.readonly
    // {scheme}://{host}/auth/{service} == full access
    // {scheme}://{host}/auth/{service}.{scope} == granular scopes

    
    public class ExternalServiceEntity : BaseEntity
    {
        public string Name { get; set; }  // service name
        public string Description { get; set; }
        public string Authority { get; set; }

    }
}

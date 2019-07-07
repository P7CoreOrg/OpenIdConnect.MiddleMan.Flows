# OpenIdConnect.MiddleMan.Flows
Sometimes just doing a login isn't enough

The OIDC.MiddleMan app calls googles [Discovery Document](https://accounts.google.com/.well-known/openid-configuration), replaces the authorization_endpoint with our own.  We are telling clients that we are the authority and thanks to the fact that the response lets us point to other endpoints helps.

```
[HttpGet]
[Route(".well-known/openid-configuration")]
public async Task<Dictionary<string, object>> GetWellknownOpenIdConfiguration()
{
    var response = await _googleDiscoveryCache.GetAsync();
    var googleStuff = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Raw);
    googleStuff["authorization_endpoint"]
       = "https://localhost:5001/connect/authorize";

    return googleStuff;
}
```  

## Configuration
When we set up [google developer credentitals](https://developers.google.com/identity/protocols/OpenIDConnect) we make sure that the redirect uri points back to the middle man.  So the client_id/client_secret belongs to the client, but the middle man is task with custodial duties. 

# External Service
```
> OpenIdConnect.Orchestrator.Flows\src\SampleExternalService>dotnet run
```  

# Google Credentials
```OIDCConsentOrchestrator->Manage User Secrets```   
```
{

  "oidcOptionStore": {
    "Google": {
      "clientRecords": {
        "1096301616546-edbl612881t7rkpljp3qa3juminskulo.apps.googleusercontent.com": {
          "secret": "{{client_secret}}"
        }
      }
    }
  }
}
```

# Orchestrator  
```
> OpenIdConnect.Orchestrator.Flows\src\OIDCConsentOrchestrator>dotnet run
```  

# Native Client
```
> OpenIdConnect.Orchestrator.Flows\src\NativeConsolePKCE-CLI\bin\Debug\netcoreapp3.1>NativeConsolePKCE-CLI.exe login -c 1234
```
The native client is hard coded to use the google client_id: ```"1096301616546-edbl612881t7rkpljp3qa3juminskulo.apps.googleusercontent.com"```
and is hard coded to request the following scopes;
```
https://www.samplecompanyapis.com/auth/sample
https://www.samplecompanyapis.com/auth/sample.readonly
https://www.samplecompanyapis.com/auth/sample.modify
```


The final task of the orchestrator is to properly mint an access token with all the authorized scopes.  At the moment I excluded the minting the access token and you only get back the one we get from Google.


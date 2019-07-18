using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OIDC.ReferenceWebClient;
using TestServerFixture;

namespace XUnitTest_Core
{
    public class MyTestServerFixture : TestServerFixture<Startup>
    {
        protected override string RelativePathToHostProject => @"../../../../OIDC.MiddleMan";
        protected override void ConfigureAppConfiguration(WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            var environmentName = hostingContext.HostingEnvironment.EnvironmentName;
            Program.LoadConfigurations(config, environmentName);
            config.AddJsonFile($"appsettings.TestServer.json", optional: true);
        }
        protected override void ConfigureServices(IServiceCollection services)
        {
           
           
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using OIDC.Orchestrator;
using RichardSzalay.MockHttp;
using TestServerFixture;

namespace XUnitTest_Core
{
    public class WithMetaData<T, TMetaData> where T : class where TMetaData : class
    {
        public TMetaData MetaData { get; }
        public T Value { get; }
        public WithMetaData(T value, TMetaData metaData)
        {
            Value = value;
            MetaData = metaData;
        }
    }
    public class MyHttpMessageHandlerFactory : IHttpMessageHandlerFactory
    {
        private List<WithMetaData<HttpMessageHandler, string>> _handlers;

        public MyHttpMessageHandlerFactory(IEnumerable<WithMetaData<HttpMessageHandler, string>> handlers)
        {
            _handlers = handlers.ToList();
        }
        public HttpMessageHandler CreateHandler(string name)
        {
            var query = from item in _handlers
                        where item.MetaData == name
                        select item.Value;
            return query.FirstOrDefault();
        }
    }

    public class MyTestServerFixture : TestServerFixture<Startup>
    {
        private MockHttpMessageHandler _mockHttp;

        string _endpoint = "https://demo.identityserver.io/.well-known/openid-configuration";
        string _jwks_uri = "https://demo.identityserver.io/.well-known/jwks";
        string _authority = "https://demo.identityserver.io";

        protected override string RelativePathToHostProject => @"../../../../OIDC.MiddleMan";

        public MyTestServerFixture() : base()
        {
            var discoFileName = FileName.Create("discovery.json");
            var document = File.ReadAllText(discoFileName);

            var jwksFileName = FileName.Create("discovery_jwks.json");
            var jwks = File.ReadAllText(jwksFileName);

            _mockHttp = new MockHttpMessageHandler();
            _mockHttp.When(_endpoint)
                .Respond("application/json", document); // Respond with JSON
            _mockHttp.When(_jwks_uri)
                .Respond("application/json", jwks); // Respond with JSON
        }
        protected override void ConfigureAppConfiguration(WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            var environmentName = hostingContext.HostingEnvironment.EnvironmentName;
            Program.LoadConfigurations(config, environmentName);
            config.AddJsonFile($"appsettings.TestServer.json", optional: true);
        }
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpMessageHandlerFactory, MyHttpMessageHandlerFactory>();

            services.AddTransient<WithMetaData<HttpMessageHandler, string>>((sp) =>
            {
                return new WithMetaData<HttpMessageHandler, string>(_mockHttp, "downstream");
            });

        }
    }
}
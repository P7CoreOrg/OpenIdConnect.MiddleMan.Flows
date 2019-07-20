using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using OIDC.ReferenceWebClient.Discovery;
using RichardSzalay.MockHttp;
using TestServerFixture;
using Xunit;

namespace XUnitTest_Core
{
    public class CoreTests : IClassFixture<MyTestServerFixture>
    {
        private MockHttpMessageHandler _mockHttp;
        /*
        string _endpoint = "https://demo.identityserver.io/.well-known/openid-configuration";
        string _jwks_uri = "https://demo.identityserver.io/.well-known/jwks";
        string _authority = "https://demo.identityserver.io";
        */
        string _endpoint = "https://accounts.google.com/.well-known/openid-configuration";
        string _jwks_uri = "https://www.googleapis.com/oauth2/v3/certs";
        string _authority = "https://accounts.google.com";

        
        private MyTestServerFixture _fixture;

        public CoreTests(MyTestServerFixture fixture)
        {
            _fixture = fixture;

            var discoFileName = FileName.Create("discovery.google.json");
            var document = File.ReadAllText(discoFileName);

            var jwksFileName = FileName.Create("discovery_jwks.google.json");
            var jwks = File.ReadAllText(jwksFileName);

            _mockHttp = new MockHttpMessageHandler();
            _mockHttp.When(_endpoint)
                .Respond("application/json", document); // Respond with JSON
            _mockHttp.When(_jwks_uri)
                .Respond("application/json", jwks); // Respond with JSON
             
        }
        [Fact]
        public void AssureFixture()
        {
            _fixture.Should().NotBeNull();
            var client = _fixture.Client;
            client.Should().NotBeNull();
        }

        [Fact]
        public async Task Base_address_should_work()
        {
 
            DownstreamDiscoveryCache dd;
            var client = new HttpClient(_mockHttp)
            {
                BaseAddress = new Uri(_endpoint)
            };

            var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest() {
                Policy = new DiscoveryPolicy
                {
                    ValidateEndpoints = false
                }
            });

            disco.IsError.Should().BeFalse();
        }

        [Fact]
        public async Task DownstreamDiscoveryCache_should_work()
        {
            var downstreamDiscoveryCache = _fixture.TestServer.Host.Services.GetRequiredService<IDownstreamDiscoveryCache>();
            downstreamDiscoveryCache.Should().NotBeNull();
            var disco = await downstreamDiscoveryCache.GetAsync();
            disco.IsError.Should().BeFalse();
        }
    }
}

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel.Client;
using TestServerFixture;
using Xunit;

namespace XUnitTest_Core
{
    public class CoreTests : IClassFixture<MyTestServerFixture>
    {
        NetworkHandler _successHandler;
        string _endpoint = "https://demo.identityserver.io/.well-known/openid-configuration";
        string _authority = "https://demo.identityserver.io";
        private MyTestServerFixture _fixture;

        public CoreTests(MyTestServerFixture fixture)
        {
            _fixture = fixture;

            var discoFileName = FileName.Create("discovery.json");
            var document = File.ReadAllText(discoFileName);

            var jwksFileName = FileName.Create("discovery_jwks.json");
            var jwks = File.ReadAllText(jwksFileName);

            _successHandler = new NetworkHandler(request =>
            {
                if (request.RequestUri.AbsoluteUri.EndsWith("jwks"))
                {
                    return jwks;
                }

                return document;
            }, HttpStatusCode.OK);
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
            var client = new HttpClient(_successHandler)
            {
                BaseAddress = new Uri(_endpoint)
            };

            var disco = await client.GetDiscoveryDocumentAsync();

            disco.IsError.Should().BeFalse();
        }
    }
}

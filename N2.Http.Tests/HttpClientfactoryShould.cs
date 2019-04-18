using Microsoft.Extensions.DependencyInjection;
using System.Security.Principal;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using N2.Http;
using N2.Http.Exceptions;

namespace XUnitHttpClientTests
{

    public class HttpClientfactoryShould
    {
        private readonly IHttpClientfactory _httpClientFactory;
        internal readonly ITestOutputHelper _outputHelper;

        public HttpClientfactoryShould(ITestOutputHelper outputHelper)
        {
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("testUser"), new[] { "testRole" });
            _httpClientFactory = Application.Services.GetService<IHttpClientfactory>();
            _outputHelper = outputHelper;
        }

        [Fact]
        public void EnableCreationOfHttpClient()
        {
            _httpClientFactory.Clear();
            IHttpClient jwtClient = _httpClientFactory.Create("jwtV1", Configuration.IdentityProvider);
            Assert.NotNull(jwtClient);
        }

        [Fact]
        public void AllowMultipleClients()
        {
            _httpClientFactory.Clear();
            IHttpClient jwtClient = _httpClientFactory.Create("jwtV1", Configuration.IdentityProvider);
            IHttpClient testClient = _httpClientFactory.Create("service", Configuration.ServiceProvider);
            Assert.Equal(2, _httpClientFactory.Length());
        }

        [Fact]
        public void ClientsHaveTheirOwnHttpClient()
        {
            _httpClientFactory.Clear();
            IHttpClient jwtClient = _httpClientFactory.Create("jwtV1", Configuration.IdentityProvider);
            IHttpClient testClient = _httpClientFactory.Create("service", Configuration.ServiceProvider);
            Assert.NotEqual(jwtClient.Client.GetHashCode(), testClient.Client.GetHashCode());
        }

        [Fact]
        public void FactoryNeedsPrincipal()
        {
            Thread.CurrentPrincipal = null;            
            Assert.Throws<PrincipalIsMissingException>(
               () =>
               {
                   _httpClientFactory.Clear();
                   _httpClientFactory.Create("jwtV1", Configuration.IdentityProvider);
               }
           );
        }

        [Fact]
        public void ClientsAreSingletonsByName()
        {
            _httpClientFactory.Clear();
            IHttpClient jwtClient1 = _httpClientFactory.CreateOrUpdate("jwtV1", Configuration.IdentityProvider);
            IHttpClient jwtClient2 = _httpClientFactory.CreateOrUpdate("jwtV1", Configuration.IdentityProvider);
            Assert.Equal(jwtClient1.GetHashCode(), jwtClient2.GetHashCode());
            Assert.Equal(jwtClient1.Client.GetHashCode(), jwtClient2.Client.GetHashCode());
        }

        [Fact]
        public void UsersHaveTheirOwnHttpClient()
        {
            _httpClientFactory.Clear();
            IHttpClient jwtClient1 = _httpClientFactory.Create("jwtV1", Configuration.IdentityProvider);
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("otherUser"), null);
            IHttpClient jwtClient2 = _httpClientFactory.Create("jwtV1", Configuration.IdentityProvider);
            Assert.NotEqual(jwtClient1.GetHashCode(), jwtClient2.GetHashCode());
            Assert.NotEqual(jwtClient1.Client.GetHashCode(), jwtClient2.Client.GetHashCode());
        }


        [Fact]
        public void MaintainUniqueClientNames()
        {
            _httpClientFactory.Clear();
            IHttpClient jwtClient = _httpClientFactory.Create("jwtV1", Configuration.IdentityProvider);
            Assert.Throws<AlreadyExistsException>(
                () => _httpClientFactory.Create("jwtV1", Configuration.IdentityProvider)
            );
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Security.Authentication;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using XUnitHttpClientTests.Models;
using N2.Http;
using N2.Http.Authorization;


namespace XUnitHttpClientTests
{
    public class HttpClientWrapperShould
    {
        private readonly IHttpClientfactory _httpClientFactory;
        internal readonly ITestOutputHelper _outputHelper;

        public HttpClientWrapperShould(ITestOutputHelper outputHelper)
        {
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("testUser"), new[] { "testRole" });
            _httpClientFactory = Application.Services.GetService<IHttpClientfactory>();
            _outputHelper = outputHelper;
        }

        [Fact]
        public async void AcceptPosts()
        {
            _httpClientFactory.Clear();
            IHttpClient jwtClient = _httpClientFactory.Create("jwtV1", Configuration.IdentityProvider);
            var result = await jwtClient.Post<dynamic, string>(new { Username = "user@mydomain.com", Password = "00000" }, "jwt");
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result));
            _outputHelper.WriteLine(result);
        }

        [Fact]
        public async void AcceptAnonymousGet()
        {
            _httpClientFactory.Clear();
            IHttpClient testClient = _httpClientFactory.Create("service", Configuration.ServiceProvider);
            var result = await testClient.Get<ServiceHealth>("/ServiceHealth");
            Assert.NotNull(result);
            _outputHelper.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        [Fact]
        public async Task ThrowUnauthorizedExceptionIfUnauthorized()
        {
            _httpClientFactory.Clear();
            IHttpClient testClient = _httpClientFactory.Create("service", Configuration.ServiceProvider);
            await Assert.ThrowsAsync<AuthenticationException>(
                () => testClient.Get<Subscription[]>("Products/70")
            );
        }

        [Fact]
        public async Task AcceptBearerTokens()
        {
            _httpClientFactory.Clear();
            IHttpClient jwtClient = _httpClientFactory.Create("jwtV1", Configuration.IdentityProvider);
            IHttpClient testClient = _httpClientFactory.Create("service", Configuration.ServiceProvider);
            var accessToken = await jwtClient.Post<dynamic, string>(new { Username= "user@mydomain.com", Password = "00000" }, "website/jwt");

            testClient.AuthorizationType = AuthorizationType.Bearer;
            testClient.BearerToken = accessToken;
            var result = await testClient.Get<Subscription[]>("Products/70");
            Assert.NotNull(result);
            _outputHelper.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        }
    }
}

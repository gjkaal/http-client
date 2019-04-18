using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using N2.Http;

namespace XUnitHttpClientTests
{
    public static class Application
    {
        public static ServiceProvider Services { get; private set; }

        static Application()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());
            services.AddSingleton<IHttpClientfactory, HttpClientfactory>();
        }
    }

}

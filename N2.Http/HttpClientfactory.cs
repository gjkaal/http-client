using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using N2.Http.Exceptions;

namespace N2.Http
{
    public class HttpClientfactory : IHttpClientfactory
    {
        private readonly static ConcurrentDictionary<string, IHttpClient> _clients = new ConcurrentDictionary<string, IHttpClient>();
        private readonly ILogger<NamedHttpClient> _logger;
        public HttpClientfactory(ILoggerFactory logger)
        {
            _logger = logger.CreateLogger<NamedHttpClient>();
        }

        public IHttpClient CreateOrUpdate(string name, string baseUrl)
        {
            var key = BuildKey(name);
            if (_clients.TryGetValue(key, out IHttpClient result))
            {
                result.SetBaseUrl(baseUrl);
                return result;
            }
            result = new NamedHttpClient(_logger, name, new HttpClient(), baseUrl);
            _clients.TryAdd(key, result);
            return result;
        }

        public IHttpClient Create(string name, string baseUrl)
        {
            var key = BuildKey(name);
            if (_clients.ContainsKey(key))
            {
                throw new AlreadyExistsException($"A client with name '{name}' already exists");
            }
            var result = new NamedHttpClient(_logger, name, new HttpClient(), baseUrl);
            _clients.TryAdd(key, result);
            return result;
        }

        public bool Clear()
        {
            var principalName = BuildKey(string.Empty);
            foreach (var key in _clients.Keys.Where(q => q.StartsWith(principalName)))
            {
                IHttpClient client=null; 
                var retries = 5;
                while (retries > 0)
                {
                    var success = _clients.TryRemove(key, out client);
                    if (success) break;
                    Thread.Sleep(42);
                    retries--;
                }
                if (client == null)
                {
                    return false;
                }
                client.Abort();
                client = null;
            }
            return true;
        }

        public int Length()
        {
            return _clients.Count();
        }

        private string BuildKey(string name)
        {
            var principal = Thread.CurrentPrincipal;
            if (principal == null) throw new PrincipalIsMissingException();
            if (principal.Identity == null) throw new PrincipalIsMissingException("Principal found, but the identity is not set");
            if (string.IsNullOrEmpty(principal.Identity.Name)) throw new PrincipalIsMissingException("Principal found, but the identity name is invalid");
            return $"{principal.Identity.Name}:{name}";
        }

    }

}

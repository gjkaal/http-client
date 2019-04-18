using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using N2.Http.Authorization;
using N2.Http.Extensions;

namespace N2.Http
{
    public class NamedHttpClient : IHttpClient
    {
        public string Name { get; private set; }
        public string BaseUrl { get; private set; }
        public HttpClient Client { get; }
        private ILogger<NamedHttpClient> _logger;

        public NamedHttpClient(ILogger<NamedHttpClient> logger, string name, HttpClient httpClient, string baseUrl)
        {
            _logger = logger;
            Client = httpClient;
            Name = name;
            if (!baseUrl.EndsWith('/')) baseUrl += '/';
            BaseUrl = baseUrl;
        }

        public AuthorizationType AuthorizationType
        {
            get { return _authorizationType; }
            set
            {
                _authorizationType = value;
                UpdateAuthorization();
            }
        }
        private AuthorizationType _authorizationType;

        public string BearerToken
        {
            get { return _bearerToken; }
            set
            {
                _bearerToken = value;
                UpdateAuthorization();
            }
        }
        private string _bearerToken;

        public BasicAuthorization BasicAuthorization 
        {
            get { return _basicAuthorization; }
            set
            {
                _basicAuthorization = value;
                UpdateAuthorization();
            }
        }
        private BasicAuthorization _basicAuthorization;

        public void SetBaseUrl(string baseUrl)
        {
            Client.CancelPendingRequests();
            BaseUrl = baseUrl;
        }

        public void Abort()
        {
            Client.CancelPendingRequests();
        }

        private void UpdateAuthorization()
        {
            const string AuthorizationHeader = "Authorization";
            if (Client.DefaultRequestHeaders.Contains(AuthorizationHeader))
            {
                Client.DefaultRequestHeaders.Remove(AuthorizationHeader);
            }

            switch (_authorizationType)
            {
                default:                    
                    break;
                case AuthorizationType.Bearer:
                    if (!string.IsNullOrEmpty(_bearerToken))
                    {
                        Client.DefaultRequestHeaders.Add(AuthorizationHeader, $"Bearer {_bearerToken}");
                    }
                    break;
                case AuthorizationType.Basic:
                    if (_basicAuthorization != null)
                    {
                        Client.DefaultRequestHeaders.Add(AuthorizationHeader, $"Basic {_basicAuthorization.Token}");
                    }
                    break;
            }
        }

        #region Get
        public async Task<TR> Get<TR>()
        {
            return await GetRequest<TR>(BaseUrl, null, default(CancellationToken));
        }
        public async Task<TR> Get<TR>(CancellationToken cancellation)
        {
            return await GetRequest<TR>(BaseUrl, null, cancellation);
        }

        public async Task<TR> Get<TR>(string path)
        {
            return await GetRequest<TR>(GetFullPath(path), null, default(CancellationToken));
        }
        public async Task<TR> Get<TR>(string path, CancellationToken cancellation)
        {
            return await GetRequest<TR>(GetFullPath(path), null, cancellation);
        }

        public async Task<TR> Get<TR>(Dictionary<string, object> queryParameters)
        {
            return await GetRequest<TR>(BaseUrl, queryParameters, default(CancellationToken));
        }
        public async Task<TR> Get<TR>(Dictionary<string, object> queryParameters, CancellationToken cancellation)
        {
            return await GetRequest<TR>(BaseUrl, queryParameters, cancellation);
        }

        public async Task<TR> Get<TR>(string path, Dictionary<string, object> queryParameters)
        {
            return await GetRequest<TR>(GetFullPath(path), queryParameters, default(CancellationToken));
        }
        public async Task<TR> Get<TR>(string path, Dictionary<string, object> queryParameters, CancellationToken cancellation)
        {
            return await GetRequest<TR>(GetFullPath(path), queryParameters, cancellation);
        }

        private async Task<TR> GetRequest<TR>(string fullPath, Dictionary<string, object> queryParameters, CancellationToken cancellation)
        {
            using (var logContext = _logger.BeginScope("GetRequest"))
            {
                _logger.Log(LogLevel.Information, $"Request: GET");
                if (queryParameters != null && queryParameters.Count > 0)
                {
                    var queryString = queryParameters.AsQueryString();
                    if (fullPath.Contains('?'))
                    {
                        fullPath = fullPath + "&" + queryString;
                    }
                    else
                    {
                        fullPath = fullPath + "?" + queryString;
                    }
                }
                _logger.Log(LogLevel.Information, $"Path: {fullPath}");

                var responseMessage = await Client.GetAsync(fullPath, cancellation);
                return await CreateResponse<TR>(fullPath, responseMessage);
            }
        }
        #endregion

        #region Post
        public async Task<TR> Post<TQ, TR>(TQ request)
        {
            return await PostRequest<TQ, TR>(BaseUrl, request, default(CancellationToken));
        }
        public async Task<TR> Post<TQ, TR>(TQ request, CancellationToken cancellation)
        {
            return await PostRequest<TQ, TR>(BaseUrl, request, cancellation);
        }

        public async Task<TR> Post<TQ, TR>(TQ request, string path)
        {
            return await PostRequest<TQ, TR>(GetFullPath(path), request, default(CancellationToken));
        }
        public async Task<TR> Post<TQ, TR>(TQ request, string path, CancellationToken cancellation)
        {
            return await PostRequest<TQ, TR>(GetFullPath(path), request, cancellation);
        }

        private async Task<TR> PostRequest<TQ, TR>(string fullPath, TQ request, CancellationToken cancellation)
        {
            using (var logContext = _logger.BeginScope("PostRequest"))
            {
                _logger.Log(LogLevel.Information, $"Request: POST {typeof(TQ)} at {fullPath}");
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var responseMessage = await Client.PostAsync(fullPath, content, cancellation);
                return await CreateResponse<TR>(fullPath, responseMessage);
            }
        }

        #endregion

        #region Tools

        private string GetFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return BaseUrl;
            }
            if (!path.Contains('?'))
            {
                if (path.StartsWith('/')) path = path.Substring(1);
                if (!path.EndsWith('/')) path = path + '/';
            }
            return string.Concat(BaseUrl, path);
        }

        private async Task<TR> CreateResponse<TR>(string fullPath, HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new AuthenticationException($"Unauthorized endpiont: {fullPath}. Check credentials");
                }

                _logger.Log(LogLevel.Warning, $"Request failed [{responseMessage.StatusCode}]:  {fullPath}");
                if (!string.IsNullOrEmpty(responseMessage.ReasonPhrase))
                {
                    _logger.Log(LogLevel.Warning, $"{fullPath}, {responseMessage.ReasonPhrase}");
                }

                // create a result
                object result;
                if (typeof(TR) == typeof(string))
                {
                    result = string.Empty;
                }
                else
                {
                    try
                    {
                        result = Activator.CreateInstance<TR>();
                    }
                    catch (Exception)
                    {
                        return default(TR);
                    }
                }

                // bind BaseResult
                var baseResult = result as BaseResult;
                if (baseResult != null)
                {
                    var response = await responseMessage.Content.ReadAsStringAsync();
                    // expect BaseResult
                    var serverResponse = JsonConvert.DeserializeObject<BaseResult>(response);
                    baseResult.Bind(serverResponse);
                    return (TR)result;
                }

                // bind as string
                var stringResult = result as string;
                if (stringResult != null)
                {
                    var response = await responseMessage.Content.ReadAsStringAsync();
                    stringResult = response;
                    return (TR)result;
                }

                return (TR)result;
            }
            else
            {
                _logger.Log(LogLevel.Information, $"Request completed [{responseMessage.StatusCode}]: {fullPath}");
                object result;
                var response = await responseMessage.Content.ReadAsStringAsync();
                if (typeof(TR) == typeof(string))
                {
                    result = response;
                }
                else
                {
                    result = JsonConvert.DeserializeObject<TR>(response);
                }
                return (TR)result;
            }
        }

        #endregion
    }

}

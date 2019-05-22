using Microsoft.Extensions.Logging;
using N2.Http.Authorization;
using N2.Http.Extensions;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace N2.Http
{


    public class NamedHttpClient : IHttpClient
    {
        public string Name { get; private set; }
        public string BaseUrl { get; private set; }
        public HttpClient Client { get; }
        private ILogger<NamedHttpClient> _logger;
        private bool _authorizationModified = false;

        private static readonly string[] ScanHeaders = new string[]
        {
            WellKnownHeaders.ETag,
            WellKnownHeaders.ContentRef,
            WellKnownHeaders.FirstPage,
            WellKnownHeaders.LastPage,
            WellKnownHeaders.NextPage,
            WellKnownHeaders.PrevPage,
        };

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
                _authorizationModified = true;
            }
        }
        private AuthorizationType _authorizationType;

        public BearerToken BearerToken
        {
            get { return _bearerToken; }
            set
            {
                _bearerToken = value;
                _authorizationModified = true;
            }
        }
        private BearerToken _bearerToken;

        public BasicAuthorization BasicAuthorization
        {
            get { return _basicAuthorization; }
            set
            {
                _basicAuthorization = value;
                _authorizationModified = true;
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

        public async Task<bool> UpdateAuthorization()
        {
            if (!_authorizationModified) return false;
            const string AuthorizationHeader = "Authorization";
            if (Client.DefaultRequestHeaders.Contains(AuthorizationHeader))
            {
                Client.DefaultRequestHeaders.Remove(AuthorizationHeader);
            }

            switch (_authorizationType)
            {
                default:
                    _authorizationModified = false;
                    return false;
                case AuthorizationType.Bearer:
                    if (_bearerToken != null )
                    {
                        var tokenValue = await _bearerToken.Token();
                        Client.DefaultRequestHeaders.Add(AuthorizationHeader, $"Bearer {tokenValue}");
                    }
                    _authorizationModified = false;
                    return true;
                case AuthorizationType.Basic:
                    if (_basicAuthorization != null)
                    {
                        Client.DefaultRequestHeaders.Add(AuthorizationHeader, $"Basic {_basicAuthorization.Token}");
                    }
                    _authorizationModified = false;
                    return true;
            }
        }

        #region Get
        public async Task<TR> Get<TR>()
        {
            var (result, status, headers) = await GetRequest<TR>(BaseUrl, null, default(CancellationToken));
            return result;
        }
        public async Task<TR> Get<TR>(CancellationToken cancellation)
        {
            var (result, status, headers) = await GetRequest<TR>(BaseUrl, null, cancellation);
            return result;
        }

        public async Task<TR> Get<TR>(string path)
        {
            var (result, status, headers) = await GetRequest<TR>(GetFullPath(path), null, default(CancellationToken));
            return result;
        }
        public async Task<TR> Get<TR>(string path, CancellationToken cancellation)
        {
            var (result, status, headers) = await GetRequest<TR>(GetFullPath(path), null, cancellation);
            return result;
        }

        public async Task<TR> Get<TR>(Dictionary<string, object> queryParameters)
        {
            var (result, status, headers) = await GetRequest<TR>(BaseUrl, queryParameters, default(CancellationToken));
            return result;
        }
        public async Task<TR> Get<TR>(Dictionary<string, object> queryParameters, CancellationToken cancellation)
        {
            var (result, status, headers) = await GetRequest<TR>(BaseUrl, queryParameters, cancellation);
            return result;
        }

        public async Task<TR> Get<TR>(string path, Dictionary<string, object> queryParameters)
        {
            var (result, status, headers) = await GetRequest<TR>(GetFullPath(path), queryParameters, default(CancellationToken));
            return result;
        }
        public async Task<TR> Get<TR>(string path, Dictionary<string, object> queryParameters, CancellationToken cancellation)
        {
            var (result, status, headers) = await GetRequest<TR>(GetFullPath(path), queryParameters, cancellation);
            return result;
        }

        private async Task<(TR, HttpStatusCode, Dictionary<string, string>)> GetRequest<TR>(string fullPath, Dictionary<string, object> queryParameters, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            await UpdateAuthorization();
            using (var logContext = _logger.BeginScope("ReadDataRequest"))
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
            var (result, status, headers) = await PostRequest<TQ, TR>(BaseUrl, request, default(CancellationToken));
            return result;
        }
        public async Task<TR> Post<TQ, TR>(TQ request, CancellationToken cancellation)
        {
            var (result, status, headers) = await PostRequest<TQ, TR>(BaseUrl, request, cancellation);
            return result;
        }

        public async Task<TR> Post<TQ, TR>(TQ request, string path)
        {
            var (result, status, headers) = await PostRequest<TQ, TR>(GetFullPath(path), request, default(CancellationToken));
            return result;
        }
        public async Task<TR> Post<TQ, TR>(TQ request, string path, CancellationToken cancellation)
        {
            var (result, status, headers) = await PostRequest<TQ, TR>(GetFullPath(path), request, cancellation);
            return result;
        }

        public async Task<(TQ, HttpStatusCode, Dictionary<string, string>)> RestPost<TQ>(TQ request, string path)
        {
            var (result, status, headers) = await PostRequest<TQ, BaseResult<TQ>>(GetFullPath(path), request, default(CancellationToken));
            return (result.Item, status, headers);
        }

        private async Task<(TR, HttpStatusCode, Dictionary<string, string>)> PostRequest<TQ, TR>(string fullPath, TQ request, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            await UpdateAuthorization();
            using (var logContext = _logger.BeginScope("CreateDataRequest"))
            {
                _logger.Log(LogLevel.Information, $"Request: POST {typeof(TQ)} at {fullPath}");
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var responseMessage = await Client.PostAsync(fullPath, content, cancellation);
                return await CreateResponse<TR>(fullPath, responseMessage);
            }
        }

        #endregion

        #region Update
        public async Task<TR> Put<TQ, TR>(TQ request)
        {
            var (result, status, headers) = await PutRequest<TQ, TR>(BaseUrl, request, default(CancellationToken));
            return result;
        }
        public async Task<TR> Put<TQ, TR>(TQ request, CancellationToken cancellation)
        {
            var (result, status, headers) = await PutRequest<TQ, TR>(BaseUrl, request, cancellation);
            return result;
        }

        public async Task<TR> Put<TQ, TR>(TQ request, string path)
        {
            var (result, status, headers) = await PutRequest<TQ, TR>(GetFullPath(path), request, default(CancellationToken));
            return result;
        }
        public async Task<TR> Put<TQ, TR>(TQ request, string path, CancellationToken cancellation)
        {
            var (result, status, headers) = await PutRequest<TQ, TR>(GetFullPath(path), request, cancellation);
            return result;
        }


        private async Task<(TR, HttpStatusCode, Dictionary<string, string>)> PutRequest<TQ, TR>(string fullPath, TQ request, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            await UpdateAuthorization();
            using (var logContext = _logger.BeginScope("UpdateDataRequest"))
            {
                _logger.Log(LogLevel.Information, $"Request: PUT {typeof(TQ)} at {fullPath}");
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var responseMessage = await Client.PutAsync(fullPath, content, cancellation);
                return await CreateResponse<TR>(fullPath, responseMessage);
            }
        }
        #endregion

        #region Delete
        public async Task<ResponseCode> Delete()
        {
            return await DeleteRequest(BaseUrl, null, default(CancellationToken));
        }
        public async Task<ResponseCode> Delete(CancellationToken cancellation)
        {
            return await DeleteRequest(BaseUrl, null, cancellation);
        }

        public async Task<ResponseCode> Delete(string path)
        {
            return await DeleteRequest(GetFullPath(path), null, default(CancellationToken));
        }
        public async Task<ResponseCode> Delete(string path, CancellationToken cancellation)
        {
            return await DeleteRequest(GetFullPath(path), null, cancellation);
        }

        public async Task<ResponseCode> Delete(Dictionary<string, object> queryParameters)
        {
            return await DeleteRequest(BaseUrl, queryParameters, default(CancellationToken));
        }
        public async Task<ResponseCode> Delete(Dictionary<string, object> queryParameters, CancellationToken cancellation)
        {
            return await DeleteRequest(BaseUrl, queryParameters, cancellation);
        }

        public async Task<ResponseCode> Delete(string path, Dictionary<string, object> queryParameters)
        {
            return await DeleteRequest(GetFullPath(path), queryParameters, default(CancellationToken));
        }

        public async Task<ResponseCode> Delete(string path, Dictionary<string, object> queryParameters, CancellationToken cancellation)
        {
            return await DeleteRequest(GetFullPath(path), queryParameters, cancellation);
        }

        private async Task<ResponseCode> DeleteRequest(string fullPath, Dictionary<string, object> queryParameters, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            await UpdateAuthorization();
            using (var logContext = _logger.BeginScope("DeleteDataRequest"))
            {
                _logger.Log(LogLevel.Information, $"Request: DELETE");
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

                var responseMessage = await Client.DeleteAsync(fullPath, cancellation);
                if (responseMessage.IsSuccessStatusCode)
                {
                    return ResponseCode.Success;
                }
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new AuthenticationException($"Unauthorized endpoint: {fullPath}. Check credentials");
                }
                _logger.Log(LogLevel.Warning, $"Request failed [{responseMessage.StatusCode}]:  {fullPath}");
                return ResponseCode.Failed;
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

        private async Task<(TR, HttpStatusCode, Dictionary<string, string>)> CreateResponse<TR>(string fullPath, HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new AuthenticationException($"Unauthorized endpoint: {fullPath}. Check credentials");
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
                        return (default(TR), responseMessage.StatusCode, GetHeaders(responseMessage.Headers));
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
                    return ((TR)result, responseMessage.StatusCode, GetHeaders(responseMessage.Headers));
                }

                // bind as string
                var stringResult = result as string;
                if (stringResult != null)
                {
                    var response = await responseMessage.Content.ReadAsStringAsync();
                    stringResult = response;
                    return ((TR)result, responseMessage.StatusCode, GetHeaders(responseMessage.Headers));
                }

                return ((TR)result, responseMessage.StatusCode, GetHeaders(responseMessage.Headers));
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

                return ((TR)result, responseMessage.StatusCode, GetHeaders(responseMessage.Headers));
            }
        }

        private Dictionary<string, string> GetHeaders(HttpResponseHeaders headers)
        {
            var result = new Dictionary<string, string>();
            foreach (var r in ScanHeaders)
            {
                if (!headers.Contains(r)) continue;
                var v = headers.GetValues(r).FirstOrDefault();
                if (string.IsNullOrEmpty(v)) continue;
                result.Add(r, v);
            }
            return result;
        }


        #endregion
    }

}

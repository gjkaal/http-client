using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using N2.Http.Authorization;

namespace N2.Http
{
    public interface IHttpClient
    {
        HttpClient Client { get; }
        AuthorizationType AuthorizationType { get; set; }
        string BearerToken { get; set; }
        BasicAuthorization BasicAuthorization { get; set; }

        Task<TR> Post<TQ, TR>(TQ request);
        Task<TR> Post<TQ, TR>(TQ request, string path);

        Task<TR> Get<TR>();
        Task<TR> Get<TR>(string path);
        Task<TR> Get<TR>(Dictionary<string, object> queryParameters);
        Task<TR> Get<TR>(string path, Dictionary<string, object> queryParameters);

        // Overload with cancellation
        Task<TR> Post<TQ, TR>(TQ request, CancellationToken cancellation);
        Task<TR> Post<TQ, TR>(TQ request, string path, CancellationToken cancellation);

        Task<TR> Get<TR>(CancellationToken cancellation);
        Task<TR> Get<TR>(string path, CancellationToken cancellation);
        Task<TR> Get<TR>(Dictionary<string, object> queryParameters, CancellationToken cancellation);
        Task<TR> Get<TR>(string path, Dictionary<string, object> queryParameters, CancellationToken cancellation);

        void SetBaseUrl(string baseUrl);
        void Abort();
    }

}

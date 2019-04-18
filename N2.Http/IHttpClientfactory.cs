namespace N2.Http
{
    public interface IHttpClientfactory
    {
        bool Clear();
        IHttpClient Create(string name, string baseUrl);
        IHttpClient CreateOrUpdate(string name, string baseUrl);
        int Length();
    }

}

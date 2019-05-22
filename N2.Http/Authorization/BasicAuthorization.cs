using System;
using System.Text;
using System.Threading.Tasks;

namespace N2.Http.Authorization
{
    public class BearerToken
    {
        private Func<Task<string>> _tokenManager;
        public BearerToken(Func<Task<string>> tokenManager)
        {
            _tokenManager = tokenManager;
        }
        public async Task<string> Token()
        {
            if (_tokenManager != null)
            {
                return await _tokenManager.Invoke();
            }
            return string.Empty;
        }
    }

    public class BasicAuthorization
    {
        public BasicAuthorization(string username, string password)
        {
            UserName = username;
            var token = $"{username}:{password}";
            byte[] basicToken = Encoding.UTF8.GetBytes(token);
            Token = Convert.ToBase64String(basicToken);
        }
        public string UserName { get; private set; }
        public string Token { get; private set; }
    }

}

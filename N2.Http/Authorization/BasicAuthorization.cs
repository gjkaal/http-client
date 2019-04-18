using System;
using System.Text;

namespace N2.Http.Authorization
{
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

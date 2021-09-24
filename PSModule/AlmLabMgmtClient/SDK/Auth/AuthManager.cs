using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.AlmLabMgmtClient.SDK.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Auth
{
    public sealed class AuthManager
    {
        private readonly IAuthenticator _restAuthenticator;
        private readonly IAuthenticator _apiKeyAuthenticator;

        private AuthManager()
        {
            _restAuthenticator = new RestAuthenticator();
            _apiKeyAuthenticator = new ApiKeyAuthenticator();
        }

        public static AuthManager Instance { get; } = new AuthManager();

        public async Task<bool> Authenticate(IClient client)
        {
            IAuthenticator auth = client.Credentials.IsSSO ? _apiKeyAuthenticator : _restAuthenticator;
            bool ok = await auth.Login(client);
            return ok;
        }
        public async Task Logout(IClient client)
        {
            IAuthenticator auth = client.Credentials.IsSSO ? _apiKeyAuthenticator : _restAuthenticator;
            _ = await auth.Logout(client);
        }

    }
}

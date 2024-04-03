/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2024 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

using PSModule.AlmLabMgmtClient.SDK.Interface;
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

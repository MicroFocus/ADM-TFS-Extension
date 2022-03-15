using PSModule.UftMobile.SDK.Interface;
using PSModule.UftMobile.SDK.Util;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;

namespace PSModule.UftMobile.SDK.Auth
{
    using C = Constants;
    public class OAuth2Authenticator : IAuthenticator
    {
        private const string OAUTH2_API = "rest/oauth2/credentials";

        public Task<bool> Login(IClient client)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Logout(IClient client)
        {
            //No logout
            return await Task.FromResult(true);
        }
    }
}

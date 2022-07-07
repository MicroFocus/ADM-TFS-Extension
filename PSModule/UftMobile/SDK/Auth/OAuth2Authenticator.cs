using PSModule.UftMobile.SDK.Interface;
using PSModule.Common;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using PSModule.UftMobile.SDK.Entity;
using PSModule.UftMobile.SDK.Enums;

namespace PSModule.UftMobile.SDK.Auth
{
    using C = Constants;
    public class OAuth2Authenticator : IAuthenticator
    {
        private const string OAUTH2_CREDENTIALS = "rest/oauth2/credentials";
        public const string OAUTH2_LOGIN = "rest/oauth2/token";

        public async Task<bool> Login(IClient client)
        {
            dynamic body = new
            {
                client = client.Credentials.UsernameOrClientId,
                secret = client.Credentials.PasswordOrSecret,
                tenant = client.Credentials.TenantId
            };
            string jsonBody = JsonConvert.SerializeObject(body);

            var res = await client.HttpPost<AccessToken>(OAUTH2_LOGIN, jsonBody, resType: ResType.Object);
            client.IsLoggedIn = res.IsOK;
            if (res.IsOK)
            {
                client.AccessToken = res.FirstEntity;
            }

            return res.IsOK;
        }

        public async Task<bool> Logout(IClient client)
        {
            //No logout
            return await Task.FromResult(true);
        }
    }
}

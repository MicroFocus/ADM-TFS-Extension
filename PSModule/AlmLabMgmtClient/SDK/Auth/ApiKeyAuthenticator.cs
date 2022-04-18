using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.Common;
using System.Net;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Auth
{
    using C = Constants;
    public class ApiKeyAuthenticator : IAuthenticator
    {
        private const string APIKEY_LOGIN_API = "rest/oauth2/login";
        private const string ALM_CLIENT_TYPE = "ALM-CLIENT-TYPE";

        public async Task<bool> Login(IClient client)
        {
            string clientId = client.Credentials.UsernameOrClientID;
            string secret = client.Credentials.PasswordOrSecret;
            string clientType = client.ClientType;
            string body = $"{{clientId:{clientId}, secret:{secret}}}";
            var headers = new WebHeaderCollection
            {
                { ALM_CLIENT_TYPE, clientType },
                { HttpRequestHeader.ContentType, C.APP_JSON},
                { HttpRequestHeader.Accept, C.APP_JSON}
            };

            await client.Logger.LogInfo("Start login to ALM server with APIkey...");

            var res = await client.HttpPost(client.ServerUrl.AppendSuffix(APIKEY_LOGIN_API), headers, body);
            bool ok = res.IsOK;
            await client.Logger.LogInfo(ok ? $"Logged in successfully to ALM Server {client.ServerUrl}"
                                  : $"Login to ALM Server at {client.ServerUrl} failed. Status Code: [{res.StatusCode}]");
            return ok;
        }

        public async Task<bool> Logout(IClient client)
        {
            //No logout
            return await Task.FromResult(true);
        }
    }
}

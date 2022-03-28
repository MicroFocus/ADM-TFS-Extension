using PSModule.UftMobile.SDK.Interface;
using PSModule.UftMobile.SDK.Util;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSModule.UftMobile.SDK.Auth
{
    using C = Constants;
    public class BasicAuthenticator : IAuthenticator
    {
        private const string LOGIN_ENDPOINT = "rest/client/login";
        private const string LOGOUT_ENDPOINT = "rest/client/logout";

        public async Task<bool> Login(IClient client)
        {
            string username = client.Credentials.UsernameOrClient;
            string pass = client.Credentials.PasswordOrSecret;

            try
            {
                string body = @$"{{""name"":""{username}"", ""password"":""{pass}"", ""accountName"": ""default""}}";
                var res = await client.HttpPost(client.ServerUrl.AppendSuffix(LOGIN_ENDPOINT), body: body);
                if (!res.IsOK)
                {
                    await client.Logger.LogDebug($"StatusCode=[{res.StatusCode}]");
                    await client.Logger.LogInfo($"{res.Error}");
                }
                return res.IsOK;
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                await client.Logger.LogError(e.Message);
                return false;
            }
        }

        public async Task<bool> Logout(IClient client)
        {
            bool isLoggedOut = false;
            if (client != null)
            {
                string body = @"{{ ""data"": {{ }}, ""error"": false, ""message"": ""string"", ""messageCode"": 0 }}";
                Response response = await client.HttpPost(client.ServerUrl.AppendSuffix(LOGOUT_ENDPOINT), body: body);
                isLoggedOut = response.IsOK;
            }
            return isLoggedOut;
        }
    }
}

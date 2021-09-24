using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.AlmLabMgmtClient.SDK.Util;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PSModule.AlmLabMgmtClient.SDK.Auth
{
    using C = Constants;
    public class RestAuthenticator : IAuthenticator
    {
        private const string USERNAME = "Username";
        private const string IS_AUTH_ENDPOINT = "rest/is-authenticated";
        private const string AUTH_ENDPOINT = "authentication-point/authenticate";
        private const string LOGOUT_ENDPOINT = "authentication-point/logout";
        private const string SESSION_ENDPOINT = "rest/site-session";
        private const string LOGGED_OUT_SUCCESSFULLY = "Logged Out Successfully.";
        private const string CHECK_IF_AUTHENTICATED = "Check if is authenticated ...";
        protected const string AUTHENTICATE_HEADER = "WWW-Authenticate";
        protected const string AUTHENTICATION_INFO = "AuthenticationInfo";

        public async Task<bool> Login(IClient client)
        {
            string username = client.Credentials.UsernameOrClientID;
            string password = client.Credentials.PasswordOrSecret;
            string clientType = client.ClientType;

            try
            {
                bool isAuthenticated = await IsAuthenticated(client, username);
                if (isAuthenticated)
                    return true;

                bool ok = await Authenticate(client, username, password);

                if (ok)
                    ok = await AppendQCSessionCookies(client, clientType);
                return ok;
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

        private async Task<bool> AppendQCSessionCookies(IClient client, string clientType)
        {
            await client.Logger.LogInfo("Creating session...");
            var headers = new WebHeaderCollection
            {
                { HttpRequestHeader.ContentType, C.APP_XML },
                { HttpRequestHeader.Accept, C.APP_XML }
            };

            // issue a post request so that cookies relevant to the QC Session will be added to the RestClient
            Response res = await client.HttpPost(
                client.ServerUrl.AppendSuffix(SESSION_ENDPOINT), 
                headers,
                $"<session-parameters><client-type>{clientType}</client-type></session-parameters>");
            bool ok = res.IsOK;
            await client.Logger.LogInfo(ok ? "Session created." : $"Cannot append QCSession cookies. Error: {res.Error}");
            return ok;
        }

        public async Task<bool> Logout(IClient client)
        {
            bool isLoggedOut = false;
            if (client != null)
            {
                // note the get operation logs us out by setting authentication cookies to:
                // LWSSO_COOKIE_KEY="" via server response header Set-Cookie
                Response response = await client.HttpGet(client.ServerUrl.AppendSuffix(LOGOUT_ENDPOINT));
                isLoggedOut = response.IsOK;
                if (isLoggedOut)
                    await client.Logger.LogInfo(LOGGED_OUT_SUCCESSFULLY);
            }
            return isLoggedOut;
        }

        private async Task<bool> Authenticate(IClient client, string username, string password)
        {
            var headers = new WebHeaderCollection
            {
                { HttpRequestHeader.Authorization, $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"))}" }
            };
            Response response = await client.HttpGet(client.ServerUrl.AppendSuffix(AUTH_ENDPOINT), headers);

            bool ok = response.IsOK;
            await client.Logger.LogInfo(ok ? $"Logged in successfully to ALM Server {client.ServerUrl} using username [{username}]"
                                 : $"Login to ALM Server at {client.ServerUrl} failed. Status Code: {response.StatusCode}");
            return ok;
        }

        private async Task<bool> IsAuthenticated(IClient client, string username)
        {
            bool ok = false;
            await client.Logger.LogInfo(CHECK_IF_AUTHENTICATED);
            string isAuthUrl = client.ServerUrl.AppendSuffix(IS_AUTH_ENDPOINT);
            using var webclient = new WebClient { Headers = new WebHeaderCollection { { HttpRequestHeader.Accept, C.APP_XML } } };
            var res = await client.HttpGet(isAuthUrl, new WebHeaderCollection { { HttpRequestHeader.Accept, C.APP_XML } });
            if (res.IsOK)
            {
                string xml = res.Data;
                await client.Logger.LogInfo(xml);
                //check the xml response
                try
                {
                    var doc = XDocument.Parse(xml);
                    string uname = doc.Root.Element(USERNAME).Value;
                    if (uname == username)
                        ok = true;
                    else
                        await client.Logger.LogError($"Username mismatch: Expected: {username}, Received: {uname}");
                }
                catch (ThreadInterruptedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    await client.Logger.LogError(e.Message);
                    //PrintHeaders(client);
                }
                await client.Logger.LogError($"Is Authenticated = {ok}");
            }
            return ok;
        }
    }
}

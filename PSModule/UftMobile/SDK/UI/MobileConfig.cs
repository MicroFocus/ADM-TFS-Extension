
namespace PSModule.UftMobile.SDK.UI
{
    public class MobileConfig
    {
        private readonly string _serverUrl;
        private readonly string _username;
        private readonly string _password;

        public string ServerUrl => _serverUrl;
        public string Username => _username;
        public string Password => _password;

        public MobileConfig(string serverUrl, string username, string password)
        {
            _serverUrl = serverUrl;
            _username = username;
            _password = password;
        }
    }
}


namespace PSModule.UftMobile.SDK.UI
{
    public class ProxyConfig: ServerConfig
    {
        private readonly bool _useCredentials;

        public bool UseCredentials => _useCredentials;

        public ProxyConfig(string serverUrl, bool useCredentials, string username, string password) : base(serverUrl, username, password)
        {
            _useCredentials = useCredentials;
        }
    }
}

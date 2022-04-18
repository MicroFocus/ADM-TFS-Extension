
namespace PSModule.UftMobile.SDK.UI
{
    public class MobileConfig: ServerConfig
    {
        private readonly bool _useProxy;
        private readonly ProxyConfig _proxyConfig;

        public bool UseProxy => _useProxy;
        public ProxyConfig ProxyConfig => _proxyConfig;

        public MobileConfig(string serverUrl, string username, string password, bool useProxy, ProxyConfig proxyConfig): base(serverUrl, username, password)
        {
            _useProxy = useProxy;
            _proxyConfig = proxyConfig;
        }
    }
}


namespace PSModule.UftMobile.SDK.UI
{
    public class MobileConfig: ServerConfig
    {
        private readonly bool _useProxy;
        private readonly ProxyConfig _proxyConfig;

        public bool UseProxy => _useProxy;
        public ProxyConfig ProxyConfig => _proxyConfig;

        public MobileConfig(ServerConfig srvConfig, bool useProxy, ProxyConfig proxyConfig): base(srvConfig.ServerUrl, srvConfig.Username, srvConfig.Password)
        {
            _useProxy = useProxy;
            _proxyConfig = proxyConfig;
        }
    }
}

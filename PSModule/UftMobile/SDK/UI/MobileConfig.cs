
namespace PSModule.UftMobile.SDK.UI
{
    public class MobileConfig: ServerConfig
    {
        private readonly bool _useProxy;
        private readonly ProxyConfig _proxyConfig;
        private readonly string _tenantId;

        public bool UseProxy => _useProxy;
        public ProxyConfig ProxyConfig => _proxyConfig;
        public string TenantId => _tenantId;

        public MobileConfig(ServerConfig srvConfig, bool useProxy, ProxyConfig proxyConfig, string tenantId = ""): base(srvConfig.ServerUrl, srvConfig.Username, srvConfig.Password)
        {
            _useProxy = useProxy;
            _proxyConfig = proxyConfig;
            _tenantId = tenantId;
        }
    }
}

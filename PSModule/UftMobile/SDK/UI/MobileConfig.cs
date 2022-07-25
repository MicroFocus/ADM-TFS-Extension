
using PSModule.UftMobile.SDK.Entity;

namespace PSModule.UftMobile.SDK.UI
{
    public class MobileConfig: ServerConfig
    {
        private readonly bool _useProxy;
        private readonly ProxyConfig _proxyConfig;
        private readonly Device _device;
        private readonly string _workDir;

        public bool UseProxy => _useProxy;
        public ProxyConfig ProxyConfig => _proxyConfig;
        public Device Device => _device;
        public string WorkDir => _workDir;
        public string MobileInfo { get; set; }

        public MobileConfig(ServerConfig srvConfig, bool useProxy, ProxyConfig proxyConfig, Device device = null, string workDir = ""): base(srvConfig)
        {
            _useProxy = useProxy;
            _proxyConfig = proxyConfig;
            _device = device;
            _workDir = workDir;
        }
    }
}

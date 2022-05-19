using System;

namespace PSModule.UftMobile.SDK.UI
{
    public class MobileResxConfig: MobileConfig
    {
        private readonly McResources _mcResx;
        private readonly bool _includeOfflineDevices;

        public bool IncludeOfflineDevices => _includeOfflineDevices;
        public McResources McResx => _mcResx;

        public MobileResxConfig(ServerConfig srvConfig, string mcResources, bool includeOfflineDevices, bool useProxy, ProxyConfig proxyConfig = null): base(srvConfig, useProxy, proxyConfig)
        {
            Enum.TryParse(mcResources, true, out _mcResx);
            _includeOfflineDevices = includeOfflineDevices;
        }
    }
}

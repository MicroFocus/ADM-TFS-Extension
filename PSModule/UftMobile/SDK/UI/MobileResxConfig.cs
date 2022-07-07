using PSModule.UftMobile.SDK.Enums;
using System;

namespace PSModule.UftMobile.SDK.UI
{
    public class MobileResxConfig: MobileConfig
    {
        private readonly Resx _resx;
        private readonly bool _includeOfflineDevices;

        public bool IncludeOfflineDevices => _includeOfflineDevices;
        public Resx Resx => _resx;

        public MobileResxConfig(ServerConfig srvConfig, string resx, bool includeOfflineDevices, bool useProxy, ProxyConfig proxyConfig = null): base(srvConfig, useProxy, proxyConfig)
        {
            Enum.TryParse(resx, true, out _resx);
            _includeOfflineDevices = includeOfflineDevices;
        }
    }
}

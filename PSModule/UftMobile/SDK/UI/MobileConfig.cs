
using PSModule.UftMobile.SDK.Entity;
using PSModule.UftMobile.SDK.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PSModule.UftMobile.SDK.UI
{
    public class MobileConfig : ServerConfig
    {
        private readonly bool _useProxy;
        private readonly ProxyConfig _proxyConfig;
        private readonly Device _device;
        private readonly AppConfig _appConfig;
        private readonly string _workDir;
        private readonly List<App> _extraApps = new();

        public bool UseProxy => _useProxy;
        public ProxyConfig ProxyConfig => _proxyConfig;
        public Device Device => _device;
        public AppType AppType => _appConfig.AppType;
        public SysApp SysApp => _appConfig.SysApp;
        public AppLine AppLine => _appConfig.AppLine;
        public List<AppLine> ExtraAppLines => _appConfig.ExtraAppsLines;
        public AppAction AppAction => _appConfig.AppAction;
        public DeviceMetrics DeviceMetrics => _appConfig.DeviceMetrics;

        public string WorkDir => _workDir;
        public string MobileInfo { get; set; }

        public App App { get; set; }
        public List<App> ExtraApps { get { return _extraApps; } }

        public MobileConfig(ServerConfig srvConfig, bool useProxy, ProxyConfig proxyConfig, Device device = null, AppConfig appConfig = null, string workDir = ""): base(srvConfig)
        {
            _useProxy = useProxy;
            _proxyConfig = proxyConfig;
            _device = device;
            _appConfig = appConfig;
            _workDir = workDir;
        }
    }
}

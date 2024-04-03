/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2024 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */


using PSModule.UftMobile.SDK.Entity;
using PSModule.UftMobile.SDK.Enums;
using System;
using System.Collections.Generic;

namespace PSModule.UftMobile.SDK.UI
{
    public class AppConfig
    {
        private AppType _appType;
        private SysApp _sysApp;
        private AppLine _app;
        private List<AppLine> _apps;
        private AppAction _appAction;
        private DeviceMetrics _deviceMetrics;

        public AppType AppType => _appType;
        public SysApp SysApp => _sysApp;
        public AppLine AppLine => _app;
        public List<AppLine> ExtraAppsLines => _apps ?? new();
        public AppAction AppAction => _appAction;
        public DeviceMetrics DeviceMetrics => _deviceMetrics;

        public AppConfig(string appType, string sysApp, AppLine app, List<AppLine> apps, DeviceMetrics deviceMetrics, AppAction appAction)
        {
            Enum.TryParse(appType, true, out _appType);
            Enum.TryParse(sysApp, true, out _sysApp);
            _app = app;
            _apps = apps;
            _appAction = appAction;
            _deviceMetrics = deviceMetrics;
        }
    }
}

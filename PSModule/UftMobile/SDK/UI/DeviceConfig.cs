/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2023 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

using PSModule.Common;
using PSModule.UftMobile.SDK.Entity;
using PSModule.UftMobile.SDK.Enums;
using System.Collections.Generic;

namespace PSModule.UftMobile.SDK.UI
{
    public class DeviceConfig(Device device = null, AppConfig appConfig = null, string workDir = "") : IConfig
    {
        public Device Device => device;
        public AppType AppType => appConfig.AppType;
        public SysApp SysApp => appConfig.SysApp;
        public AppLine AppLine => appConfig.AppLine;
        public List<AppLine> ExtraAppLines => appConfig.ExtraAppsLines;
        public AppAction AppAction => appConfig.AppAction;
        public DeviceMetrics DeviceMetrics => appConfig.DeviceMetrics;

        public string WorkDir => workDir;
        public string MobileInfo { get; set; }

        public App App { get; set; }
        public List<App> ExtraApps { get; } = [];
    }
}

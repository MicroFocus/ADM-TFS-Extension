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

namespace PSModule.UftMobile.SDK.UI
{
    public class CloudBrowserConfig(string url, string region, string os, string browser, string version) : IConfig
    {
        public string Url => url;
        public string Region => region;
        public string OS => os;
        public string Browser => browser;
        public string Version => version;
    }
}

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

using PSModule.UftMobile.SDK.Enums;

namespace PSModule.UftMobile.SDK.UI
{
    public class ProxyConfig(ServerConfig srvConfig, bool useCredentials)
    {
        public bool UseCredentials => useCredentials;
        public AuthType AuthType => srvConfig.AuthType;
        public string ServerUrl => srvConfig.ServerUrl;
        public string UsernameOrClientId => srvConfig.UsernameOrClientId;
        public string PasswordOrSecret => srvConfig.PasswordOrSecret;
        public int TenantId => srvConfig.TenantId;
    }
}

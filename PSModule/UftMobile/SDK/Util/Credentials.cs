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

using PSModule.Properties;
using System.Management.Automation;

namespace PSModule.UftMobile.SDK.Util
{
    public class Credentials
    {
        private readonly string _usernameOrClientId;
        private readonly string _passwordOrSecret;
        private readonly int _tenantId;

        public string UsernameOrClientId => _usernameOrClientId;
        public string PasswordOrSecret => _passwordOrSecret;
        public int TenantId => _tenantId;

        public Credentials(string usernameorClientId, string passwordOrSecret, int tenantId = 0)
        {
            if (usernameorClientId.IsNullOrWhiteSpace())
                throw new UftMobileException(Resources.MissingUsernameOrClientId, ErrorCategory.InvalidArgument);

            _usernameOrClientId = usernameorClientId;
            _passwordOrSecret = passwordOrSecret;
            _tenantId = tenantId;
        }
    }
}

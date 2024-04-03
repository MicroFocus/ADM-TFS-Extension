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

using System.Management.Automation;

namespace PSModule.AlmLabMgmtClient.SDK.Util
{
    public class Credentials
    {
        private readonly bool _isSSO;
        private readonly string _usernameOrClientId;
        private readonly string _passwordOrSecret;
        public Credentials(bool isSSO, string usernameOrClientId, string passwordOrSecret)
        {
            if (usernameOrClientId.IsNullOrWhiteSpace())
                throw new AlmException("Missing username / clientId.", ErrorCategory.InvalidArgument);
            if (isSSO && passwordOrSecret.IsNullOrWhiteSpace())
                throw new AlmException("Missing Api Key Secret.", ErrorCategory.InvalidArgument);

            _isSSO = isSSO;
            _usernameOrClientId = usernameOrClientId;
            _passwordOrSecret = passwordOrSecret;
        }
        public bool IsSSO => _isSSO;
        public string UsernameOrClientID => _usernameOrClientId;
        public string PasswordOrSecret => _passwordOrSecret;

    }
}

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

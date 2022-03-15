using System.Management.Automation;

namespace PSModule.UftMobile.SDK.Util
{
    public class Credentials
    {
        private readonly bool _isOAuth2;
        private readonly string _usernameOrClientId;
        private readonly string _passwordOrSecret;
        public Credentials(string usernameOrClientId, string passwordOrSecret, bool isSSO = false)
        {
            if (usernameOrClientId.IsNullOrWhiteSpace())
                throw new UftMobileException("Missing username / clientId.", ErrorCategory.InvalidArgument);
            if (isSSO && passwordOrSecret.IsNullOrWhiteSpace())
                throw new UftMobileException("Missing Api Key Secret.", ErrorCategory.InvalidArgument);

            _isOAuth2 = isSSO;
            _usernameOrClientId = usernameOrClientId;
            _passwordOrSecret = passwordOrSecret;
        }
        public bool IsOAuth2 => _isOAuth2;
        public string UsernameOrClient => _usernameOrClientId;
        public string PasswordOrSecret => _passwordOrSecret;

    }
}

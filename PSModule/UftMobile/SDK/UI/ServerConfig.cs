
using PSModule.Properties;
using PSModule.UftMobile.SDK.Enums;
using System;
using C = PSModule.Common.Constants;

namespace PSModule.UftMobile.SDK.UI
{
    public class ServerConfig
    {
        private readonly string _serverUrl;
        private readonly string _usernameOrClientId;
        private readonly string _passwordOrSecret;
        private readonly string _tenantId;
        private readonly AuthType _authType;

        public AuthType AuthType => _authType;
        public string ServerUrl => _serverUrl;
        public string UsernameOrClientId => _usernameOrClientId;
        public string PasswordOrSecret => _passwordOrSecret;
        public string TenantId => _tenantId;

        public ServerConfig(string serverUrl, string usernameOrClientId, string passwordOrSecret, string tenantId, AuthType authType)
        {
            _serverUrl = serverUrl.Trim();
            _usernameOrClientId = usernameOrClientId;
            _passwordOrSecret = passwordOrSecret;
            _tenantId = tenantId;
            _authType = authType;
        }

        public ServerConfig(string serverUrl, string usernameOrClientId, string passwordOrSecret, string tenantId = "", bool isBasicAuth = true) : 
            this(serverUrl, usernameOrClientId, passwordOrSecret, tenantId, isBasicAuth ? AuthType.Basic : AuthType.AccessKey)
        {
        }
        /// <summary>
        /// Parses the execution token and separates into three parts: clientId, secretKey and tenantId
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ParseAccessKey(string accessKey, out string clientId, out string secret, out string tenantId)
        {
            clientId = secret = tenantId = string.Empty;

            // exec token consists of three parts:
            // 1. client id
            // 2. secret key
            // 3. optionally tenant id
            // separator is ;
            // key-value pairs are separated with =

            // e.g., "client=oauth2-QHxvc8bOSz4lwgMqts2w@microfocus.com; secret=EHJp8ea6jnVNqoLN6HkD; tenant=999999999;"
            // "client=oauth2-OuV8k3snnGp9vJugC1Zn@microfocus.com; secret=6XSquF1FUD4CyQM7fb0B; tenant=999999999;"
            // "client=oauth2-OuV8k3snnGp9vJugC1Zn@microfocus.com; secret=6XSquF1FUD7CyQM7fb0B; tenant=999999999;"

            accessKey = accessKey.Trim().Trim(C.DOUBLE_QUOTE_);
            if (accessKey.IsNullOrEmpty()) return Resources.McMissingOrInvalidAcessKey;

            var tokens = accessKey.Split(C.SEMI_COLON_, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length != 3) throw new ArgumentException(Resources.McInvalidToken);

            // key-values are separated by =, we need its value, the key is known
            foreach (var token in tokens)
            {
                var parts = token.Split(C.EQUAL);

                if (parts.Length != 2)
                    return string.Format(Resources.McMalformedTokenMissingKeyValuePair, token);

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                if (key.EqualsIgnoreCase(C.CLIENT))
                {
                    clientId = value;
                }
                else if (key.EqualsIgnoreCase(C.SECRET))
                {
                    secret = value;
                }
                else if (key.EqualsIgnoreCase(C.TENANT))
                {
                    tenantId = value;
                }
                else
                {
                    return string.Format(Resources.McMalformedTokenInvalidKey, key);
                }
            }
            return string.Empty;
        }

    }
}

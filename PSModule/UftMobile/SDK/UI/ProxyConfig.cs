
namespace PSModule.UftMobile.SDK.UI
{
    public class ProxyConfig: ServerConfig
    {
        private readonly bool _useCredentials;

        public bool UseCredentials => _useCredentials;

        public ProxyConfig(ServerConfig srvConfig, bool useCredentials) : base(srvConfig)
        {
            _useCredentials = useCredentials;
        }
    }
}

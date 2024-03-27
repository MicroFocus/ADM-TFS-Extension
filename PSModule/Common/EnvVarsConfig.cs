
using PSModule.UftMobile.SDK.UI;

namespace PSModule.Common
{
    public class EnvVarsConfig(string storageAccount, string container, string leaveUftOpenIfVisible = "") : IConfig
    {
        private readonly string _storageAccount = storageAccount;
        private readonly string _container = container;
        private readonly string _leaveUftOpenIfVisible = leaveUftOpenIfVisible;

        public string StorageAccount => _storageAccount;
        public string Container => _container;
        public string LeaveUftOpenIfVisible => _leaveUftOpenIfVisible;
    }
}


using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Interface
{
    public interface ILogger
    {
        public Task LogInfo(string msg);
        public Task LogDebug(string msg);
        public Task ShowProgress();
        public Task LogError(string err, [CallerMemberName] string methodName = "");

    }
}

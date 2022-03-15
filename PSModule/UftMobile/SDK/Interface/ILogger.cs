
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PSModule.UftMobile.SDK.Interface
{
    public interface ILogger
    {
        bool IsDebug { get; }

        public Task LogInfo(string msg);
        public Task LogDebug(string msg);
        public Task ShowProgress();
        public Task LogError(string err, [CallerMemberName] string methodName = "");

    }
}

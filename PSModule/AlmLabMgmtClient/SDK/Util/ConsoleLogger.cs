using PSModule.AlmLabMgmtClient.SDK.Interface;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Util
{
    public class ConsoleLogger : ILogger
    {
        private const string PLEASE_WAIT = "Please wait ....";
        private readonly bool _isDebug;

        public ConsoleLogger(bool isDebug)
        {
            _isDebug = isDebug;
        }

        public async Task LogInfo(string msg)
        {
            await Console.Out.WriteLineAsync(msg);
        }
        public async Task LogDebug(string msg)
        {
            if (_isDebug)
            {
                await Console.Out.WriteLineAsync(msg);
            }
        }

        public async Task LogError(string err, [CallerMemberName] string methodName = "")
        {
            await Console.Error.WriteLineAsync($"{methodName}: {err}");
        }

        public async Task ShowProgress()
        {
            await Console.Out.WriteLineAsync(PLEASE_WAIT);
        }
    }
}

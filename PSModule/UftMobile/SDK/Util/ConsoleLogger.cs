using PSModule.UftMobile.SDK.Interface;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PSModule.UftMobile.SDK.Util
{
    public class ConsoleLogger : ILogger
    {
        private const string PLEASE_WAIT = "Please wait ....";
        private readonly bool _isDebug;

        public bool IsDebug => _isDebug;

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
            if (_isDebug)
                await Console.Error.WriteLineAsync($"{methodName}: {err}");
            else
                await Console.Error.WriteLineAsync($"{err}");
        }

        public async Task ShowProgress()
        {
            await Console.Out.WriteLineAsync(PLEASE_WAIT);
        }
    }
}

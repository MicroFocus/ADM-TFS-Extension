using PSModule.AlmLabMgmtClient.SDK.Handler;
using PSModule.AlmLabMgmtClient.SDK.Interface;
using PSModule.AlmLabMgmtClient.SDK.Util;
using PSModule.Common;
using System.Management.Automation;

namespace PSModule.AlmLabMgmtClient.SDK.Factory
{
    using C = Constants;
    public class RunHandlerFactory
    {
        public RunHandler Create(IClient client, string runType, string entityId)
        {
            return runType switch
            {
                C.BVS => new BvsRunHandler(client, entityId),
                C.TEST_SET => new TestSetRunHandler(client, entityId),
                _ => throw new AlmException("RunHandlerFactory: Run type {runType} is Not Implmented", ErrorCategory.NotImplemented),
            };
        }
    }
}
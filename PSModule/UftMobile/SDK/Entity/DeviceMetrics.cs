using Newtonsoft.Json;
using C = PSModule.Common.Constants;

namespace PSModule.UftMobile.SDK.Entity
{
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class DeviceMetrics
    {
        private bool cpu;
		private bool memory;
        private bool freeMemory;
        private bool logs;
        private bool wifiState;
        private bool thermalState;
        private bool freeDiskSpace;
        //private bool screenshot;
        //private bool wifiSignalStrength;
        //private bool totalDiskSpace;

        public DeviceMetrics(bool cpu, bool memory, bool freeMemory, bool logs, bool wifiState, bool thermalState, bool freeDiskSpace)
        {
            this.cpu = cpu;
            this.memory = memory;
            this.freeMemory = freeMemory;
            this.logs = logs;
            this.wifiState = wifiState;
            this.thermalState = thermalState;
            this.freeDiskSpace = freeDiskSpace;
        }
    }
}

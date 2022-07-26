
using Newtonsoft.Json;
using System.Collections.Generic;
using C = PSModule.Common.Constants;
namespace PSModule.UftMobile.SDK.Entity
{
    public class MobileInfo
    {
        private static readonly char[] _escapeChars = new char[] { C.BACK_SLASH, C.COLON };

        //private const string DEFAULT_HEADER = "{{\"collect\":{{\"cpu\":false,\"memory\":false,\"freeMemory\":false,\"logs\":false,\"wifiState\":false,\"thermalState\":false,\"freeDiskSpace\":false,\"wifiSignalStrength\":false,\"screenshot\":false}},\"configuration\":{{\"installAppBeforeExecution\":false,\"deleteAppAfterExecution\":false,\"restartApp\":false}}";

        [JsonProperty]
        private readonly string id;
        //private readonly App application;
        [JsonProperty]
        private readonly List<Device> devices;
        //private readonly string header;
        [JsonProperty]
        private readonly CapableDeviceFilterDetails capableDeviceFilterDetails;
        //private App[] extraApps;

        public MobileInfo(string jobId, Device device = null, CapableDeviceFilterDetails cdfDetails = null)
        {
            id = jobId;
            devices = device == null ? null : new() { device };
            capableDeviceFilterDetails = cdfDetails;
        }

        public override string ToString()
        {
            return this.ToJson(false, true).Escape(_escapeChars);
        }
    }
}

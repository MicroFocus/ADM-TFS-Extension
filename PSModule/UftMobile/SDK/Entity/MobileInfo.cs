
using System.Collections.Generic;
using C = PSModule.Common.Constants;
namespace PSModule.UftMobile.SDK.Entity
{
    public class MobileInfo
    {
        private static readonly char[] _escapeChars = new char[] { C.BACK_SLASH, C.COLON };

        //private const string DEFAULT_HEADER = "{{\"collect\":{{\"cpu\":false,\"memory\":false,\"freeMemory\":false,\"logs\":false,\"wifiState\":false,\"thermalState\":false,\"freeDiskSpace\":false,\"wifiSignalStrength\":false,\"screenshot\":false}},\"configuration\":{{\"installAppBeforeExecution\":false,\"deleteAppAfterExecution\":false,\"restartApp\":false}}";

        public string Id { get; set; }
        public App Application { get; set; }
        public List<Device> Devices { get; set; } = new();
        public string Header { get; set; }
        public CapableDeviceFilterDetails CapableDeviceFilterDetails { get; set; }
        public App[] ExtraApps { get; set; }

        public override string ToString()
        {
            return this.ToJson(false, true).Escape(_escapeChars);
        }
    }
}

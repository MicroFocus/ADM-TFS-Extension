
using Newtonsoft.Json;
using System.Collections.Generic;
using C = PSModule.Common.Constants;
namespace PSModule.UftMobile.SDK.Entity
{
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class MobileInfo
    {
        private static readonly char[] _escapeChars = new char[] { C.BACK_SLASH, C.COLON };

        private readonly string id;
        private readonly App application;
        private readonly List<Device> devices;
        private readonly string header;
        private readonly CapableDeviceFilterDetails capableDeviceFilterDetails;
        private List<App> extraApps;

        public MobileInfo(string jobId, Device device = null, CapableDeviceFilterDetails cdfDetails = null, App app = null, List<App> extraApps = null, string hdr = null)
        {
            id = jobId;
            devices = device == null ? null : new() { device };
            capableDeviceFilterDetails = cdfDetails;
            application = app;
            this.extraApps = extraApps;
            header = hdr;
        }

        public override string ToString()
        {
            return this.ToJson(_escapeChars);
        }
    }
}

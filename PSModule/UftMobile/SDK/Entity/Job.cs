
using Newtonsoft.Json;
using C = PSModule.Common.Constants;

namespace PSModule.UftMobile.SDK.Entity
{
    public class Job
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Status { get; set; }
        public App Application { get; set; }
        public Device[] Devices { get; set; }
        public string Header { get; set; }
        public CapableDeviceFilterDetails CapableDeviceFilterDetails { get; set; }
        public App[] ExtraApps { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, C.JsonSerializerSettings);
        }
    }
}

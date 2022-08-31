
using Newtonsoft.Json;

namespace PSModule.UftMobile.SDK.Entity
{
    public class Header
    {
        [JsonProperty("collect")]
        public DeviceMetrics DeviceMetrics { get; set; }
        [JsonProperty("configuration")]
        public AppAction AppAction { get; set; }
    }
}

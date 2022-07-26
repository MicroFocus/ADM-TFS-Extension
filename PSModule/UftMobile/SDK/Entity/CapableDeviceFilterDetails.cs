
namespace PSModule.UftMobile.SDK.Entity
{
    public class CapableDeviceFilterDetails
    {
        private const string ANY = "Any";

        public string Source { get; set; } = ANY;
        public string Udid { get; set; }
        public string DeviceName { get; set; }
        public string PlatformName { get; set; }
        public string PlatformVersion { get; set; }
        public string FleetType { get; set; } = ANY.ToLower();
    }
}

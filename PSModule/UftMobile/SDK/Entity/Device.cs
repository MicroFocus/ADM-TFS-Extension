
namespace PSModule.UftMobile.SDK.Entity
{
    public class Device
    {
        public string DeviceId { get; set; }
        public string Model { get; set; }
        public string OSType { get; set; }
        public string OSVersion { get; set; }
        public string Manufacturer { get; set; }
        public string DeviceStatus { get; set; }

        public override string ToString()
        {
            return $"deviceId: {DeviceId}, manufacturer: {Manufacturer}, model: {Model}, osType: {OSType}, osVersion: {OSVersion}";
        }
    }
}

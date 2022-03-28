
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
            return $@"DeviceID: ""{DeviceId}"", Manufacturer: ""{Manufacturer}"", Model: ""{Model}"", OSType: ""{OSType}"", OSVersion: ""{OSVersion}""";
        }
        public string ToRawString()
        {
            return $@"deviceId: {DeviceId}, manufacturerAndModel: {Manufacturer} {Model}, osType: {OSType}, osVersion: {OSVersion}";
        }
    }
}

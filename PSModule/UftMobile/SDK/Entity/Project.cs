
using Newtonsoft.Json;

namespace PSModule.UftMobile.SDK.Entity
{
    public class Project
    {
        [JsonProperty("tenantId")]
        public int Id { get; set; }
        [JsonProperty("tenantName")]
        public string Name { get; set; }
        [JsonProperty("isActive")]
        public bool IsActive { get; set; }
    }
}

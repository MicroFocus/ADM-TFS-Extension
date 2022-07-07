using System;
using Newtonsoft.Json;

namespace PSModule.UftMobile.SDK.Entity
{
    public class AccessToken
    {
        private readonly DateTime _creationTime = DateTime.Now;

        [JsonProperty("access_token")]
        public string Value { get; set; }
        [JsonProperty("token_type")]
        public string Type { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        public bool IsExpired()
        {
            if (DateTime.Now.Subtract(_creationTime).TotalSeconds >= ExpiresIn)
                return true;

            return false;
        }
    }
}

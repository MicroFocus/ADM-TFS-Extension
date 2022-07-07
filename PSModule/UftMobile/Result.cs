using Newtonsoft.Json;

namespace PSModule.UftMobile
{
    public class Result
    {
        public int MessageCode { get; set; }
        public string Message { get; set; }
        public bool Error { get; set; }
    }
    public class MultiResult<T> : Result
    {
        [JsonProperty("data")]
        public T[] Entries { get; set; }
    }
    public class SingleResult<T> : Result
    {
        [JsonProperty("data")]
        public T Entry { get; set; }
    }
}

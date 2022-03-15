namespace PSModule.UftMobile
{
    public class Result
    {
        public int MessageCode { get; set; }
        public string Message { get; set; }
        public bool Error { get; set; }
    }
    public class Result<T> : Result
    {
        public T[] Data { get; set; }
    }
}

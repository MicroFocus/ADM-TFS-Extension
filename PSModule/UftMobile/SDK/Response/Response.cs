using Newtonsoft.Json;
using System;
using System.Net;

namespace PSModule.UftMobile.SDK
{
    public class Response
    {
        private readonly string _error;
        protected readonly WebHeaderCollection _headers;
        protected readonly HttpStatusCode? _statusCode;

        public WebHeaderCollection Headers => _headers;
        public string Error => _error;
        public HttpStatusCode? StatusCode => _statusCode;

        protected Response()
        {
        }

        public Response(string err, HttpStatusCode? statusCode = null)
        {
            _error = err;
            _statusCode = statusCode;
        }
        public Response(string body, WebHeaderCollection headers, HttpStatusCode statusCode) : this(headers, statusCode)
        {
            var res = JsonConvert.DeserializeObject<Result>(body);
            if (res.Error)
            {
                _error = res.Message;
            }
        }

        public Response(WebHeaderCollection headers, HttpStatusCode statusCode)
        {
            _headers = headers;
            _statusCode = statusCode;
        }
        public bool IsOK => _error == null && _statusCode.In(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted);

    }

    public class Response<T> : Response
    {
        private readonly string _error;
        private readonly T[] _entries;
        public T[] Entities => _entries ?? new T[0];
        public new string Error => _error;
        public Response(string body, WebHeaderCollection headers, HttpStatusCode statusCode) : base(headers, statusCode)
        {
            Result<T> res = JsonConvert.DeserializeObject<Result<T>>(body);
            _entries = res.Data;
            if (res.Error)
            {
                _error = res.Message;
            }
        }
        public Response(string err, HttpStatusCode? statusCode = null) : base(err, statusCode)
        {
        }
    }
}

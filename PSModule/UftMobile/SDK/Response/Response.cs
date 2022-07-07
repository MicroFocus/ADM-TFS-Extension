using Newtonsoft.Json;
using PSModule.UftMobile.SDK.Enums;
using System;
using System.Net;

namespace PSModule.UftMobile.SDK
{
    public class Response
    {
        protected string _error;
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
        public bool IsOK => _error.IsNullOrWhiteSpace() && _statusCode.In(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted);

    }

    public class Response<T> : Response where T : class
    {
        private readonly T[] _entries;
        public T[] Entities => _entries ?? new T[0];
        public T FirstEntity => _entries?[0];
        public Response(string body, WebHeaderCollection headers, HttpStatusCode statusCode, ResType resType) : base(headers, statusCode)
        {
            switch (resType)
            {
                case ResType.DataEntities:
                    {
                        var res = JsonConvert.DeserializeObject<MultiResult<T>>(body);
                        _entries = res.Entries;
                        if (res.Error)
                        {
                            _error = res.Message;
                        }
                        break;
                    }
                case ResType.DataEntity:
                    {
                        var res = JsonConvert.DeserializeObject<SingleResult<T>>(body);
                        _entries = new T[] { res.Entry };
                        if (res.Error)
                        {
                            _error = res.Message;
                        }
                        break;
                    }
                default:
                    {
                        var res = JsonConvert.DeserializeObject<T>(body);
                        _entries = new T[] { res };
                        break;
                    }
            }
        }
        public Response(string err, HttpStatusCode? statusCode = null) : base(err, statusCode)
        {
        }
    }
}

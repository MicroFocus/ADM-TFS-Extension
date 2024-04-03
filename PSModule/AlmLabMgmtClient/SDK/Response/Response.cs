/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2024 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

using System.Net;

namespace PSModule.AlmLabMgmtClient.SDK
{
    public class Response
    {
        private readonly WebHeaderCollection _headers;
        private readonly string _data;
        private readonly string _error;
        private readonly HttpStatusCode? _statusCode;
        public WebHeaderCollection Headers => _headers;
        public string Data => _data;
        public string Error => _error;
        public HttpStatusCode? StatusCode => _statusCode;

        public Response()
        {
        }

        public Response(string err, HttpStatusCode? statusCode = null)
        {
            _error = err;
            _statusCode = statusCode;
        }

        public Response(string data, WebHeaderCollection headers, HttpStatusCode statusCode)
        {
            _headers = headers;
            _data = data;
            _statusCode = statusCode;
        }

        public Response(WebHeaderCollection headers, HttpStatusCode statusCode)
        {
            _headers = headers;
            _statusCode = statusCode;
        }
        public bool IsOK => _error == null && _statusCode.In(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted);

        public override string ToString()
        {
            return _data;
            //return Encoding.UTF8.GetString(Data);
        }

    }
}

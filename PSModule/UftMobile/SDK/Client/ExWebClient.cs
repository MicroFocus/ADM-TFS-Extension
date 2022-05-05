using System;
using System.Net;

namespace PSModule.UftMobile.SDK.Client
{
    public sealed class ExWebClient : WebClient
    {
        public string Method
        {
            get;
            set;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);

            if (!Method.IsNullOrEmpty())
                webRequest.Method = Method;

            return webRequest;
        }
    }
}

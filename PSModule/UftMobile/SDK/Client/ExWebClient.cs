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

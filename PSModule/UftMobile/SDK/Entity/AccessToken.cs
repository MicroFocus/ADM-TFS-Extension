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

﻿/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2023 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

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

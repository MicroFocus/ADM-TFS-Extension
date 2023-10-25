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

using System;
using System.Management.Automation;
using System.Runtime.Serialization;

namespace PSModule.UftMobile.SDK.Util
{
    [Serializable]
    public class UftMobileException : Exception
    {
        private readonly ErrorCategory _category;
        public UftMobileException(string message, ErrorCategory categ = ErrorCategory.NotSpecified) : base(message)
        {
            _category = categ;
        }
        protected UftMobileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // https://rules.sonarsource.com/csharp/RSPEC-3925
        }
        public ErrorCategory Category { get; }

    }
}

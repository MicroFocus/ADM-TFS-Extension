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


using PSModule.Common;

namespace PSModule.UftMobile.SDK.Enums
{
    public enum SysApp
    {
        None,
        [StringValue("MC.Browser")]
        Browser,
        [StringValue("MC.Settings")]
        Settings,
        [StringValue("MC.SMS")]
        SMS,
        [StringValue("MC.Phone")]
        Phone,
        [StringValue("MC.Mail")]
        Mail,
        [StringValue("MC.Calendar")]
        Calendar,
        [StringValue("MC.Camera")]
        Camera
    }
}

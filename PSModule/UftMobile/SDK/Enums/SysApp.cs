
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

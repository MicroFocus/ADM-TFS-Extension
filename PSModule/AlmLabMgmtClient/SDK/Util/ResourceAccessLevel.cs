
using PSModule.Common;

namespace PSModule.AlmLabMgmtClient.SDK.Util
{
    public enum ResourceAccessLevel
    {
        [StringValue(null)]
        PUBLIC,
        [StringValue("PtAL")]
        PROTECTED,
        [StringValue("PvAL")]
        PRIVATE
    }
}

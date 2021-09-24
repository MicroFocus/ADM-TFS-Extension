using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Util
{
    public class Credentials
    {
        public bool IsSSO { get; internal set; }
        public string UsernameOrClientID { get; internal set; }
        public string PasswordOrSecret { get; internal set; }

    }
}

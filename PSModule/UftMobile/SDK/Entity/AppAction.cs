
using Newtonsoft.Json;

namespace PSModule.UftMobile.SDK.Entity
{
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class AppAction
    {
        private bool installAppBeforeExecution;
        private bool deleteAppAfterExecution;
        private bool restartApp;

        public AppAction(bool install, bool uninstall, bool restart)
        {
            installAppBeforeExecution = install;
            deleteAppAfterExecution = uninstall;
            restartApp = restart;
        }
    }
}

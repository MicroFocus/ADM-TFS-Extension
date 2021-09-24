using System.Threading.Tasks;

namespace PSModule.AlmLabMgmtClient.SDK.Interface
{
    public interface IAuthenticator
    {
        Task<bool> Login(IClient client);
        Task<bool> Logout(IClient client);

    }
}

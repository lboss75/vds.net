using System.Threading.Tasks;

namespace IVySoft.VDS.Client
{
    public interface IVdsApi : System.IDisposable
    {
        Task Login(string login, string password);
        Task<ChannelMessage[]> GetChannels();

    }
}
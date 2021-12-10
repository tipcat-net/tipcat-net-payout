using System.Threading.Tasks;

namespace TipCatDotNet.Payout.Services.HttpClients
{
    public interface IPayoutHttpClient
    {
        Task Payout();
    }
}

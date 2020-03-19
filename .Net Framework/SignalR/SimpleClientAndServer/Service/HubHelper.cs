using Microsoft.AspNet.SignalR;
using Interfaces;

namespace Service
{
    public class HubHelper
    {
        public static IHubContext<IClientMethods> GetContext<T>() where T : BaseHub
        {
            return GlobalHost.ConnectionManager.GetHubContext<T, IClientMethods>();
        }
    }
}

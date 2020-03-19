using Interfaces;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Service
{
    [HubName("Demo")]
    public class BaseHub : Hub<IClientMethods>, IServiceMethods
    {

    }

}

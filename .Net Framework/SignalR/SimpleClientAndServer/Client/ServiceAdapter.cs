using Interfaces;

namespace Client
{
    internal class ServiceAdapter : IServiceMethods
    {
        private readonly HubProxy _proxy;
        internal ServiceAdapter(HubProxy proxy)
        {
            _proxy = proxy;
        }
    }
}

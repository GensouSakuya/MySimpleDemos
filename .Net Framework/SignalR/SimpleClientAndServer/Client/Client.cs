using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace Client
{
    public abstract class Client
    {
        public IServiceMethods Service { get; }

        protected Client(string host, int port, string hubName, Dictionary<string, string> qsData,
            bool useHttps = true, bool throwExceptionWhenFailed = false)
        {
            try
            {
                var proxy = new HubProxy("localhost", port, hubName, qsData, false, BindMethodToProxy);
                Service = new ServiceAdapter(proxy);
                proxy.Start();
            }
            catch
            {
                if (throwExceptionWhenFailed)
                {
                    throw;
                }
            }
        }

        public void Dispose()
        {
            Service?.Dispose();
        }

        internal abstract void BindMethodToProxy(HubProxy proxy);
    }
}

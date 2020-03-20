using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Communication;

namespace WcfService
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new RobotService(new WcfAdapter());
            service.Run();
            Console.Read();
        }
    }

    public class WCFService : RobotService
    {
        public WCFService() : base(new WcfAdapter())
        {
            this.CallerResolver = () => OperationContext.Current.GetCallbackChannel<IRobotClient>();
        }
    }

    public class WcfAdapter : ServiceAdapter
    {
        private ServiceHost _serviceHost;
        public override void Run()
        {
            _serviceHost = new ServiceHost(typeof(WCFService), new Uri("net.pipe://localhost/NamedPipeDuplexServer"));
            _serviceHost.AddServiceEndpoint(typeof(IRobotService),
                new NetNamedPipeBinding(NetNamedPipeSecurityMode.None), "net.pipe://localhost/NamedPipeDuplexServer/EncooService");
            var serviceMetadataBehavior = _serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (serviceMetadataBehavior == null)
            {
                serviceMetadataBehavior = new ServiceMetadataBehavior();
                _serviceHost.Description.Behaviors.Add(serviceMetadataBehavior);
            }

            _serviceHost.AddServiceEndpoint(typeof(IMetadataExchange),
                MetadataExchangeBindings.CreateMexNamedPipeBinding(),
                $"EncooService/mex");

            if (_serviceHost.State != CommunicationState.Opened)
                _serviceHost.Open();
        }
    }
}

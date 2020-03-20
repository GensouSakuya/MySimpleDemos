using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Communication;
using WCF;

namespace WcfClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new RobotClient(new WcfAdapter());
            client.ServiceProxy.Value.SendMessage("ohhhhhhhhhhhh");
            Console.Read();
        }
    }
    public class WcfAdapter : ClientAdapter
    {
        public override void Connect()
        {
            var factory = new DuplexChannelFactory<IRobotService>(typeof(RobotClient),
                new NetNamedPipeBinding(NetNamedPipeSecurityMode.None), "net.pipe://localhost/NamedPipeDuplexServer/EncooService");
            this.ServiceProxy = factory.CreateChannel(new InstanceContext((RobotClient)this.Client));
        }
    }

}

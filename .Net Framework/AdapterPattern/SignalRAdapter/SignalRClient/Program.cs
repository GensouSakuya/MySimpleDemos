using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communication;

namespace SignalRClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new RobotClient(new SignalRAdapter());
            client.ServiceProxy.Value.SendMessage("ohhhhhhhhhhhh");
            Console.Read();
        }
    }
}

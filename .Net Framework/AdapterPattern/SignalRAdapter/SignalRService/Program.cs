using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communication;

namespace SignalRCom
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new RobotService(new SignalRAdapter());
            service.Run();
            Console.Read();
        }
    }
}

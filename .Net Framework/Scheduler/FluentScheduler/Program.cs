using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    public class Job : IJob
    {
        public void Execute()
        {
            Console.WriteLine("executing...");
        }
    }
}

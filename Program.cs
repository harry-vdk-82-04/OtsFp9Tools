using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ots.cmd;
using Ots.fp9;

namespace Ots
{
    class Program
    {
        private const string Version = "1.01";
        static void Main(string[] args)
        {
            Console.Out.WriteLine("OtsFp9Tools. v" + Version);

            var cmd = Parcer.ReadCommandLine();
            cmd.Run();
        }
    }
}

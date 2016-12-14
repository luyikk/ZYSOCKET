using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.MicroThreading;

namespace ScriptDEMO
{
    class Program
    {
        static void Main(string[] args)
        {
            ScriptSystem system = new ScriptSystem(1);

            system.Add(new ScriptDEMO.AsynScript1());
            system.Add(new ScriptDEMO.AsynScript2());
            system.Add(new ScriptDEMO.AsynScript3());
            system.Start();


            Console.ReadLine();
        }
    }
}

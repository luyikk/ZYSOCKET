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
            ScriptSystem system = new ScriptSystem(0);
            AsynScript1 a;
            AsynScript2 b;
            AsynScript3 c;
            AsynScript4 d;
            system.Add(a = new ScriptDEMO.AsynScript1());
            system.Add(b = new ScriptDEMO.AsynScript2());
            system.Add(c = new ScriptDEMO.AsynScript3());
            system.Add(d = new ScriptDEMO.AsynScript4());
            system.Start();


            Console.ReadLine();
            system.Remove(a);
            Console.ReadLine();
            system.Remove(b);
            Console.ReadLine();
            system.Remove(c);
            Console.ReadLine();
        }
    }
}

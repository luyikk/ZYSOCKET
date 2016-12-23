using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZYSocket.MicroThreading;

namespace ScriptDEMO
{
    public class AsynScript2:AsyncScript
    {

        private string GetThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId.ToString() + ":";
        }

        public override async Task Execute()
        {
            while (true)
            {
                await System.NextFrame();

                //await Task.Delay(1000);

                Console.WriteLine(GetThreadId()+DateTime.Now);

            }
        }
    }
}

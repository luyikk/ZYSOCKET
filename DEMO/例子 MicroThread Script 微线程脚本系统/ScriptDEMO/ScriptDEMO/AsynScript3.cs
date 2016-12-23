using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZYSocket.MicroThreading;
using ZYSocket.ZYCoroutinesin;

namespace ScriptDEMO
{
    public class AsynScript3 : AsyncScript
    {

        Fiber fiber;

        public AsynScript3()
        {
            fiber = new Fiber();
            fiber.SetAction(async () =>
            {
                while(true)
                {
                    
                    await Fiber.Current.Send<string>("1");

                    await Fiber.Current.Send<string>("2");

                    await Fiber.Current.Send<string>("3");

                    await Fiber.Current.Send<string>("4");

                    await Fiber.Current.Send<string>("5--DOWN HTML START");

                }
            });
            fiber.Start();
        }

        private string GetThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId.ToString() + ":";
        }


        public async  Task<string> GetArgs()
        {
            string a = await fiber.Read<string>();
            await fiber.Back<string>();
            return a;
        }



        public  override async Task Execute()
        {
            while(true)
            {
                await System.NextFrame();

                string a = await GetArgs();              

                Console.WriteLine(GetThreadId()+"Id:{0}", a);

                //await Task.Delay(2000);
            }
        }
    }
}

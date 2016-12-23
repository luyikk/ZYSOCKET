using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.MicroThreading;
using System.Net;
using ZYSocket.ZYCoroutinesin;
using System.Diagnostics;
using System.Threading;

namespace ScriptDEMO
{
    public class AsynScript1:AsyncScript
    {

    
        public async Task Down()
        {
            while (true)
            {
                string url = await Fiber.Current.Get<string>();

                Debug.WriteLine("Current TID:" + Thread.CurrentThread.ManagedThreadId);

                WebClient client = new WebClient();
                string html = client.DownloadString(url);               

                await Fiber.Current.Send<string>(html);
            }
            
        }

        public async Task<string> DownHtml(Fiber fiber,string url)
        {
            await fiber.Set<string>(url);
            string htm = await fiber.Read<string>();           
            await fiber.Back<string>();

            return htm;
        }


        private string GetThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId.ToString()+":";
        }


        public override async Task Execute()
        {
            while (true)
            {
                await System.NextFrame();

                Console.WriteLine(GetThreadId() + "Start Down");

                Fiber fiber = new Fiber();
                fiber.SetAction(Down);
                fiber.Start();


                string baidu = await DownHtml(fiber, "http://www.baidu.com");

                string QQ = await DownHtml(fiber, "http://www.QQ.com");


                Console.WriteLine(GetThreadId()+"Baidu HTML LENGTH:" + baidu.Length);

                Console.WriteLine(GetThreadId() + "QQ HTML LENGTH:" + QQ.Length);

                Console.WriteLine(GetThreadId() + "Down Close");

                //await Task.Delay(10000);                
              

            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.MicroThreading;

namespace ScriptDEMO
{
    public class AsynScript4 : AsyncScript
    {
        public override async Task Execute()
        {
            while (true)
            {
                await SystemCore.NextFrame();


                WebClient client = new WebClient();
                byte[] a = await client.DownloadDataTaskAsync("http://www.baidu.com");

                Console.WriteLine(a.Length);

                await Task.Delay(10000);
            }
        }
    }
}

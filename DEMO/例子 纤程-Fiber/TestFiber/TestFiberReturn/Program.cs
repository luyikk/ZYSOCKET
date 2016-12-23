using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.share;
using ZYSocket.ZYCoroutinesin;

namespace TestFiberReturn
{
    class Program
    {

        static string datax = "";

        static async Task<string> GetString()
        {
            //SEND

            await Fiber.Current.Send<string>("CC");

            string a= await Fiber.Current.Get<string>();

            return a;
        }


        static async Task<string> GetString2()
        {
            await Task.Delay(1);

            await fiber.Send<string>("CC");


            return "123123";

        }



        static async Task<string> Run1()
        {
            return await GetString();
        }

        static Fiber fiber = new Fiber();


        static void Main(string[] args)
        {
            Run0();

            Console.ReadLine();

            Run3();


            Console.ReadLine();
        }



        public static async void Run3()
        {
            await Task.Factory.StartNew(async () =>
            {
                await fiber.Set<string>(datax);

            });
           
        }


        public static async void Run0()
        {
          


            fiber.SetAction(async () =>
            {
                // string res = await Run1();
                string a= await GetString2();
                Console.WriteLine(a);

            });

            fiber.Start();

            if (!fiber.IsOver)
            {
                string data = await fiber.Read<string>();

                data += " OK";

                await fiber.Back<string>();
                
                datax = data;
            }

        }
    }
}

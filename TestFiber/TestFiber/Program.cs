using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.ZYCoroutinesin;

namespace TestFiber
{
    class Program
    {
        static Fiber tmp = new Fiber();
        static void Main(string[] args)
        {
            tmp.SetAction(async () =>
            {

                int a = await Fiber.Current.Get<int>();
                string b = await Fiber.Current.Get<string>();
                float c = await Fiber.Current.Get<float>();
                double d = await Fiber.Current.Get<double>();
                Console.WriteLine("{0}-{1}-{2}-{3}", a, b, c, d);
                await Fiber.Current.Send<string>("处理完成");

                //===============第2章====================
                Console.WriteLine("我又回来了");

                int a2 = await Fiber.Current.Get<int>();
                string b2 = await Fiber.Current.Get<string>();
                float c2 = await Fiber.Current.Get<float>();
                double d2 = await Fiber.Current.Get<double>();
                Console.WriteLine("{0}-{1}-{2}-{3}", a2, b2, c2, d2);
                await Fiber.Current.Send<string>("我又处理完成了");
                Console.WriteLine("结束");
            });

            tmp.Start();
            Send();
            Console.ReadLine();

            tmp.Start();
            Send();

            Console.ReadLine();
        }

        static async void Send()
        {
            await tmp.Set<int>(1);
            await tmp.Set<string>("2");
            await tmp.Set<float>(3.0f);
            await tmp.Set<double>(4.0f);
            string a = await tmp.Read<string>();

            Console.WriteLine(a);

            await tmp.Back<string>();
            await tmp.Set<int>(5);
            await tmp.Set<string>("6");
            await tmp.Set<float>(7.0f);
            await tmp.Set<double>(8.0f);
            a = await tmp.Read<string>();
            Console.WriteLine(a);
            await tmp.Back<string>();
        }
    }
}

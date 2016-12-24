using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.MicroThreading;

namespace TestMicroThread
{
    class Program
    {
        static Scheduler Threads = new Scheduler();

        static Channel test = new Channel();

        static void Main(string[] args)
        {


            Scheduler scehduler = new Scheduler();

            scehduler.Add(async () =>
                {
                    await Task.Delay(1000);
                    Console.WriteLine("AAAAAAAA");
                });


            scehduler.Run();
            System.Threading.Thread.Sleep(1100);
            scehduler.Run();

            Console.ReadLine();
            

            ScriptSystem tmp = new ScriptSystem(0);

            var a1 = new Test2() { Name = "1", Priority = 2, testcab = test };
            var a2 = new Test2() { Name = "2", Priority = 1, testcab = test };
            var a3 = new Test() { Name = "3", Priority = 0, testcab = test };
            var a4 = new Test3() { Name = "4", Priority = 0, testcab = test };

            tmp.Add(a1);
            tmp.Add(a2);
            tmp.Add(a3);
            tmp.Add(a4);



            tmp.Start();


            Console.ReadLine();

            tmp.Stop();

            Console.ReadLine();

            tmp.Start();

            Console.ReadLine();

            tmp.Remove(a1);
            tmp.Remove(a2);
            tmp.Remove(a3);
            tmp.Remove(a4);

            Console.ReadLine();



        }




    }




    public class Test2 : SyncScript
    {
        public string Name { get; set; }

        public Channel testcab { get; set; }

        public override void Start()
        {
            Console.WriteLine("This " + Name);

        }

        public override void Update()
        {

            testcab.Set<string>(Name);
        }

        public override void Cancel()
        {
            Console.WriteLine(Name + " 被删除了");
        }

    }

    public class Test : AsyncScript
    {
        public string Name { get; set; }


        public Channel testcab { get; set; }

        public override async Task Execute()
        {
            Console.WriteLine("My Name is " + Name);

            while (true)
            {
                string a = await testcab.Get<string>();
                Console.WriteLine(a);

            }
        }

        public override void Cancel()
        {
            Console.WriteLine(Name + " 被删除了");
        }

    }

    public class Test3 : AsyncScript
    {
        public string Name { get; set; }


        public Channel testcab { get; set; }

        public override async Task Execute()
        {

            Console.WriteLine("My Name is " + Name);

            while (true)
            {
                await SystemCore.NextFrame();

                testcab.SetSync<string>(Name);

            }
        }

        public override void Cancel()
        {
            Console.WriteLine(Name + " 被删除了");
        }

    }
}

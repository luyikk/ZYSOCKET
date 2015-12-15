using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPC;

namespace RPCTest
{
    public class TestUserLogOn:RPCObject
    {

        public bool IsLogON(string username, out string message)
        {

            message = username + "登入成功:"+GetCurrentSocketAsynEvent().AcceptSocket.RemoteEndPoint.ToString();
            return true;

        }

        public List<UserData> GetData()
        {
            List<UserData> tmp = new List<UserData>();

            Random r=new Random();

            for (int i = 0; i < 10; i++)
            {
                tmp.Add(new UserData()
                {
                    M1=r.Next(),
                    M2=r.Next(),
                    M3=r.Next(),
                    M4=r.Next()
                });

            }

            return tmp;
        }


        public string GetMyIP()
        {          
            return GetCurrentSocketAsynEvent().AcceptSocket.RemoteEndPoint.ToString();
        }

        

    }

    public class UserData
    {
        public int M1 { get; set; }
        public int M2 { get; set; }
        public int M3 { get; set; }
        public int M4 { get; set; }

    }


    public class ITest
    {

        public void Run(string name)
        {

            Console.WriteLine("Call args:" + name);

        }

        public int GetRes(int i)
        {
            return i + 1;
        }

        public List<string> GetResOP(List<string> re)
        {
            return re;
        }

    }



    public class TestTow
    {
        public int R { get; set; }

        public DateTime GetTime()
        {
            return DateTime.Now;
        }


        public void SetR(int v)
        {
            this.R = v;
        }

        public int GetR()
        {
            return this.R;
        }

        public bool TestRef(ref string a)
        {
            a = "ref+" + a;

            return true; ;
        }

        public void TestOut(out string a, string b)
        {
            a = "out+" + b;
        }
    }

}

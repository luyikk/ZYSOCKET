using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using ZYSocket.SSLClientB;

namespace ConsoleApplication7
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = "mybank1.icbc.com.cn";
            SocketSSLClient tp = new SocketSSLClient();           
            tp.GetCerts += new GetCertHandler(tp_GetCerts);
            tp.ErrorLog += new ErrorLogOutHandler(tp_ErrorLog);
            bool p = tp.Connect("mybank1.icbc.com.cn", 443);

      
            if (p)
            {
                tp.BinaryInput += new ClientBinaryInputHandler(tp_BinaryInput);
                tp.StartRead();


                byte[] data = Encoding.UTF8.GetBytes("GET /icbc/perbank/index.jsp HTTP/1.1 \r\nAccept: */*\r\nhost:" + host + "\r\n\r\n");

                tp.Send(data);
            }

            Console.ReadLine();
        }

        static X509CertificateCollection tp_GetCerts()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            return store.Certificates;
        }

        static void tp_BinaryInput(byte[] data)
        {
            string a = Encoding.Default.GetString(data);

            Console.WriteLine(a);
        }

        static void tp_ErrorLog(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}

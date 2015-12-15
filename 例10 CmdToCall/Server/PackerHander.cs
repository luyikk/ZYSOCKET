using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.share;
using System.Net.Sockets;
using BuffLibrary;
using ZYSocket.Server;

namespace Server
{
    public class PackerHander : ICmdToCall
    {

        #region 全局静态唯一对象
        static object lockthis = new object();

        static PackerHander _My;

        public static PackerHander GetInstance()
        {
            lock (lockthis)
            {

                if (_My == null)
                    _My = new PackerHander();
            }

            return _My;
        }

        private PackerHander()
        {

        }
        #endregion


        public void Loading()
        {
            //-----------------------------------------手动添加-----------------------------------------------------------
            CmdToCallManager<ZYSocketSuper, int, ReadBytes, SocketAsyncEventArgs>.GetInstance().AddCall(1000, LogOn);
            CmdToCallManager<ZYSocketSuper, int, ReadBytes, SocketAsyncEventArgs>.GetInstance().AddCall(800, Ping);
            CmdToCallManager<ZYSocketSuper, int, ReadBytes, SocketAsyncEventArgs>.GetInstance().AddCall(1002, ReadDataSet);

            //------------------------------------------标签自动添加CMD函数指针-------------------------------------------
            CmdToCallManager<ZYSocketSuper, ReadBytes, SocketAsyncEventArgs>.GetInstance().AddPackerObj(this);
            //-----------------------------------------注意2个类是不一样的------------------------------------------------
        }

       
        [CmdTypeOfAttibutes(1000)]
        public void LogOn(ZYSocketSuper server, ReadBytes read, SocketAsyncEventArgs socketAsync)
        {
            Login p;

            if (read.ReadObject<Login>(out p))
            {

                if (p != null)
                {
                    if (User.UserManger.GetUserDataManger().CheckUser(p.UserName, p.PassWord))//检查用户名密码是否正确
                    {
                        User.UserInfo user = new User.UserInfo() //建立一个新的用户对象 并且初始化 用户名
                        {
                            UserName = p.UserName
                        };

                        socketAsync.UserToken = user; //设置USERTOKEN

                        Message err = new Message() //初始化MESSAGE数据包类
                        {
                            Type = 2,
                            MessageStr = "登入成功"
                        };

                        server.SendData(socketAsync.AcceptSocket, BufferFormat.FormatFCA(err)); //发送此类

                        Console.WriteLine(user.UserName + " 登入");

                    }
                    else
                    {
                        Message err = new Message() //初始化用户名密码错误数据包
                        {
                            Type = 1,
                            MessageStr = "用户名或密码错误"
                        };

                        server.SendData(socketAsync.AcceptSocket, BufferFormat.FormatFCA(err));
                    }

                }
            }
        }
               
        [CmdTypeOfAttibutes(800)]
        public void Ping(ZYSocketSuper server, ReadBytes read, SocketAsyncEventArgs socketAsync)
        {
            Ping pdata;
            if (read.ReadObject<Ping>(out pdata)) //读取PING 数据包
            {
                if (pdata != null)
                {
                    pdata.ServerReviceTime = DateTime.Now; //设置服务器时间
                    server.SendData(socketAsync.AcceptSocket, BufferFormat.FormatFCA(pdata)); //发送返回
                }
            }
        }
      
        [CmdTypeOfAttibutes(1002)]
        public void ReadDataSet(ZYSocketSuper server, ReadBytes read, SocketAsyncEventArgs socketAsync)
        {
            ReadDataSet rd;

            if (read.ReadObject<ReadDataSet>(out rd)) //读取请求DATASET 数据包
            {

                if (rd != null)
                {
                    rd.Data = new List<DataValue>();

                    rd.TableName = "table1";
                    rd.Data.Add(new DataValue()
                    {
                        V1 = "第1个",
                        V2 = "第2个",
                        V3 = "第3个",
                        V4 = "第4个",
                        V5 = "第5个"
                    });

                    rd.Data.Add(new DataValue()
                    {
                        V1 = "第6个",
                        V2 = "第7个",
                        V3 = "第8个",
                        V4 = "第9个",
                        V5 = "第10个"
                    });

                    rd.Data.Add(new DataValue()
                    {
                        V1 = "第11个",
                        V2 = "第12个",
                        V3 = "第13个",
                        V4 = "第14个",
                        V5 = "第15个"
                    });


                    rd.Data.Add(new DataValue()
                    {
                        V1 = "第16个",
                        V2 = "第17个",
                        V3 = "第18个",
                        V4 = "第19个",
                        V5 = "第20个"
                    });

                    server.SendData(socketAsync.AcceptSocket, BufferFormat.FormatFCA(rd)); //发送

                    Console.WriteLine((socketAsync.UserToken as User.UserInfo).UserName + " 读取了" + rd.TableName);
                }


            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
namespace ZYSocket.share
{
    /// <summary>
    /// 新的数据包格式化类，非常简洁 详细请看DEMO  AutoBufferDemo
    /// </summary>
    public class ZYAutoBuffer
    {
        public Dictionary<int, List<Type>> CallsArgsTable { get; private set; }
        public Dictionary<int,MethodInfo> CallsMethods { get; private set; }

       

        public ZYAutoBuffer(Type packHandlerType)
        {
            InitMethod(packHandlerType);
        }
        
        private void InitMethod(Type packHandlerType)
        {
            CallsMethods = new Dictionary<int, MethodInfo>();
            CallsArgsTable = new Dictionary<int, List<Type>>();

            var methods = packHandlerType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);


            foreach (var method in methods)
            {
                var attr = method.GetCustomAttributes(typeof(CmdTypeOfAttibutes), true);

                if (attr.Length == 1)
                {
                    CmdTypeOfAttibutes attrcmdtype = attr[0] as CmdTypeOfAttibutes;

                    if (attrcmdtype != null)
                    {
                        if (!CallsMethods.ContainsKey(attrcmdtype.CmdType))
                        {
                            CallsMethods.Add(attrcmdtype.CmdType, method);

                            var types= method.GetParameters();

                            List<Type> tmplist = new List<Type>(types.Length);

                            foreach (var item in types)
                            {
                                tmplist.Add(item.ParameterType);
                            }

                            CallsArgsTable.Add(attrcmdtype.CmdType, tmplist);
                        }
                    }
                }
            }
        }


        public void Run(ReadBytesV2 read)
        {
            Run((ReadBytes)read);
        }

        public void Run(ReadBytes read)
        {
            int cmd = read.ReadInt32();

            if (CallsMethods.ContainsKey(cmd))
            {

                AutoBuffer buffer = read.ReadObject<AutoBuffer>();

                object[] args = null;

                if(CallsArgsTable.ContainsKey(cmd))
                {
                    var argsTypeTable = CallsArgsTable[cmd];

                    if (argsTypeTable.Count>0&&argsTypeTable.Count==buffer.Args.Count)
                    {
                        args = new object[argsTypeTable.Count];

                        for (int i = 0; i < argsTypeTable.Count; i++)
                        {
                            args[i] = UnpackSingleObject(argsTypeTable[i], buffer.Args[i]);
                        }
                    }

                    CallsMethods[cmd].Invoke(null, args);
                }

                
            }
            
        }


        public void Run<T>(ReadBytesV2 read,T obj)
        {
             Run<T>((ReadBytes)read, obj);
        }


        public void Run<T>(ReadBytes read,T obj)
        {
            int cmd = read.ReadInt32();

            if (CallsMethods.ContainsKey(cmd))
            {

                AutoBuffer buffer = read.ReadObject<AutoBuffer>();

                object[] args = null;

                if (CallsArgsTable.ContainsKey(cmd))
                {
                    var argsTypeTable = CallsArgsTable[cmd];

                    if (argsTypeTable.Count > 0 && argsTypeTable.Count == (buffer.Args.Count+1))
                    {
                        args = new object[argsTypeTable.Count];

                        args[0] = obj;
                        int x = 1;
                        for (int i = 0; i < (argsTypeTable.Count-1); i++)
                        {
                            x = i + 1;
                            args[x] = UnpackSingleObject(argsTypeTable[x], buffer.Args[i]);
                        }
                       
                    }

                    CallsMethods[cmd].Invoke(null, args);
                }


              

            }
        }


               

        public void Run<T1,T2>(ReadBytes read, T1 obj1,T2 obj2)
        {
            int cmd = read.ReadInt32();

            if (CallsMethods.ContainsKey(cmd))
            {

                AutoBuffer buffer = read.ReadObject<AutoBuffer>();

                object[] args = null;

                if (CallsArgsTable.ContainsKey(cmd))
                {
                    var argsTypeTable = CallsArgsTable[cmd];

                    if (argsTypeTable.Count > 0 && argsTypeTable.Count == (buffer.Args.Count + 2))
                    {
                        args = new object[argsTypeTable.Count];

                        args[0] = obj1;
                        args[1] = obj2;

                        int x = 2;
                        for (int i = 0; i < (argsTypeTable.Count - 2); i++)
                        {
                            x = i + 2;
                            args[x] = UnpackSingleObject(argsTypeTable[x], buffer.Args[i]);
                        }                       
                    }

                    CallsMethods[cmd].Invoke(null, args);
                }


              

            }
        }


        public void Run<T1, T2,T3>(ReadBytes read, T1 obj1, T2 obj2,T3 obj3)
        {
            int cmd = read.ReadInt32();

            if (CallsMethods.ContainsKey(cmd))
            {

                AutoBuffer buffer = read.ReadObject<AutoBuffer>();

                object[] args = null;

                if (CallsArgsTable.ContainsKey(cmd))
                {
                    var argsTypeTable = CallsArgsTable[cmd];

                    if (argsTypeTable.Count > 0 && argsTypeTable.Count == (buffer.Args.Count + 3))
                    {
                        args = new object[argsTypeTable.Count];

                        args[0] = obj1;
                        args[1] = obj2;
                        args[2] = obj3;

                        int x = 3;
                        for (int i = 0; i < (argsTypeTable.Count - 3); i++)
                        {
                            x = i + 3;
                            args[x] = UnpackSingleObject(argsTypeTable[x], buffer.Args[i]);
                        }

                       
                    }

                    CallsMethods[cmd].Invoke(null, args);

                }


               

            }
        }


        public static byte[] Call(int cmd,params object[] args)
        {
            return Call(cmd, null, args);
        }


        public static byte[] Call(int cmd, FDataExtraHandle dataExtra, params object[] args)
        {
            AutoBuffer buffer = new AutoBuffer()
            {
                Args = new List<byte[]>(args.Length)
            };


            foreach (var item in args)
            {
                Type type = item.GetType();

                buffer.Args.Add(PackSingleObject(type, item));

            }


            using (MemoryStream stream = new MemoryStream())
            {

                BinaryWriter bufflist = new BinaryWriter(stream);

                if (dataExtra != null)
                {

                    bufflist.Write(cmd);
                    byte[] classdata = BufferFormat.SerializeObject(buffer);
                    bufflist.Write(classdata.Length);
                    bufflist.Write(classdata);

                    byte[] fdata = dataExtra(stream.ToArray());

                    stream.Position = 0;
                    stream.SetLength(0);
                    bufflist.Write(0);
                    bufflist.Write(fdata);
                }
                else
                {
                    bufflist.Write(0);
                    bufflist.Write(cmd);
                    byte[] classdata = BufferFormat.SerializeObject(buffer);
                    bufflist.Write(classdata.Length);
                    bufflist.Write(classdata);

                }

                int l = (int)(stream.Length);

                byte[] data = BufferFormat.GetSocketBytes(l);

                stream.Position = 0;

                bufflist.Write(data);


                byte[] pdata = stream.ToArray();
                stream.Close();
                stream.Dispose();

                return pdata;

            }
        }



        public static byte[] CallV2(int cmd, params object[] args)
        {
            return CallV2(cmd, null, args);
        }


        public static byte[] CallV2(int cmd, FDataExtraHandle dataExtra, params object[] args)
        {
            AutoBuffer buffer = new AutoBuffer()
            {
                Args = new List<byte[]>(args.Length)
            };


            foreach (var item in args)
            {
                Type type = item.GetType();

                buffer.Args.Add(PackSingleObject(type, item));

            }


            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter bufflist = new BinaryWriter(stream);

                bufflist.Write(BufferFormatV2.GetBytes(cmd));

                byte[] classdata = BufferFormatV2.SerializeObject(buffer);
                bufflist.Write(BufferFormatV2.GetBytes(classdata.Length));
                bufflist.Write(classdata);


                byte[] fdata = null;

                if (dataExtra != null)
                {
                    fdata = dataExtra(stream.ToArray());
                }
                else
                {
                    fdata = stream.ToArray();
                }

                stream.Position = 0;
                stream.SetLength(0);



                int x = fdata.Length;

                if ((fdata.Length + 1) < 128)
                {
                    x += 1;
                }
                else if ((fdata.Length + 2) < 16384)
                {
                    x += 2;
                }
                else if ((fdata.Length + 3) < 2097152)
                {
                    x += 3;
                }
                else
                {
                    x += 4;
                }

                byte[] tmp = BufferFormatV2.GetBytes(x);

                int l = fdata.Length + tmp.Length;

                byte[] data = BufferFormatV2.GetBytes(l);

                bufflist.Write((byte)0xFF);
                bufflist.Write(data);
                bufflist.Write(fdata);

                byte[] pdata = stream.ToArray();
                stream.Close();
                stream.Dispose();
                return pdata;
            }
        }



        public static object UnpackSingleObject(Type type, byte[] data)
        {

            if (type == typeof(string))
            {
                return ReadString(data);
            }
            else if (type == typeof(byte[]))
            {
                return data;
            }
            else if (type.BaseType == typeof(Array))
            {
                List<byte[]> list = (List<byte[]>)ProtoUnpackSingleObject(typeof(List<byte[]>), data);

                Type memberType = type.GetMethod("Get").ReturnType;

                var array = Array.CreateInstance(memberType, list.Count);


                for (int i = 0; i < list.Count; i++)
                {
                    array.SetValue(UnpackSingleObject(memberType, list[i]), i);
                }

                return array;
            }
            else
                return ProtoUnpackSingleObject(type, data);


        }

        public static byte[] PackSingleObject(Type type, object obj)
        {

            if (type == typeof(string))
            {
                return Encoding.UTF8.GetBytes((string)obj);
            }
            else if (type == typeof(byte[]))
            {
                return (byte[])obj;
            }
            else if (type.BaseType == typeof(Array))
            {
                Array array = (Array)obj;

                List<byte[]> arlist = new List<byte[]>(array.Length);

                for (int i = 0; i < array.Length; i++)
                {
                    arlist.Add(PackSingleObject(array.GetValue(i).GetType(), array.GetValue(i)));
                }

                return ProtoBufPackSingleObject(arlist);
            }
            else
                return ProtoBufPackSingleObject(obj);


        }

        public static byte[] ProtoBufPackSingleObject(object obj)
        {
            using (System.IO.MemoryStream _memory = new System.IO.MemoryStream())
            {
                ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(_memory, obj);

                return _memory.ToArray();

            }
        }

        public static object ProtoUnpackSingleObject(Type type, byte[] data)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(data))
            {
                return ProtoBuf.Serializer.Deserialize(type, stream);
            }
        }

        #region  return 字符串
        /// <summary>
        /// 读取内存流中一段字符串
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static string ReadString(byte[] Data)
        {

            string values = Encoding.UTF8.GetString(Data);

            return values;

        }
        #endregion


        public class AutoBuffer
        {
            public List<byte[]> Args { get; set; }
        }
    }
}

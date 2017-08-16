using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace ZYSocket.share
{
    public class BufferFormatV2:BufferFormat
    {
        public BufferFormatV2(int buffType, FDataExtraHandle dataExtra):base(buffType,dataExtra)
        {
            stream = new MemoryStream();
            buffList = new BinaryWriter(stream);
            buffList.Write(GetBytes(buffType));
            
            Encode = Encoding.Unicode;
            finish = false;
            this.dataextra = dataExtra;       
        
        }        

        public BufferFormatV2(int buffType):base(buffType)
        {
            stream = new MemoryStream();
            buffList = new BinaryWriter(stream);

            buffList.Write(GetBytes(buffType));
            Encode = Encoding.Unicode;
            finish = false;
        }

        public override void AddItem(int data)
        {
            for (; ; )
            {
                if ((data & ~127) == 0)
                {
                    AddItem((byte)data);
                    return;
                }
                AddItem((byte)(data & 127 | 128));
                data = data >> 7;
            }          
        }

        public override void AddItem(object obj)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            byte[] data = SerializeObject(obj);
            buffList.Write(GetBytes(data.Length));
            buffList.Write(data);
        }



        public override void AddItem(string data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            Byte[] bytes = Encode.GetBytes(data);
            buffList.Write(GetBytes(bytes.Length));
            buffList.Write(bytes);
        }


        public override void AddItem(byte[] data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");
                      
            buffList.Write(GetBytes(data.Length));
            buffList.Write(data);
        }

        public override byte[] Finish()
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            byte[] fdata = null;

            if (dataextra != null)
            {
                fdata = dataextra(stream.ToArray());
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

            byte[] tmp = GetBytes(x);

            int l = fdata.Length + tmp.Length;

            byte[] data = GetBytes(l);

            buffList.Write((byte)0xFF);
            buffList.Write(data);
            buffList.Write(fdata);

            byte[] pdata = stream.ToArray();
#if !COREFX
            stream.Close();
#endif
            stream.Dispose();
            finish = true;
            return pdata;
          
        }

        /// <summary>
        /// 直接格式化一个带FormatClassAttibutes 标签的类，返回BYTE[]数组，此数组可以直接发送不需要组合所数据包。所以也称为类抽象数据包
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static new  byte[] FormatFCA(object o)
        {
            return FormatFCA(o, null);
        }

         /// <summary>
        /// 直接格式化一个带FormatClassAttibutes 标签的类，返回BYTE[]数组，此数组可以直接发送不需要组合所数据包。所以也称为类抽象数据包
       /// </summary>
       /// <param name="o"></param>
       /// <param name="dataExtra">数据加密回调</param>
       /// <returns></returns>
        public static new  byte[] FormatFCA(object o, FDataExtraHandle dataExtra)
        {
            Type otype = o.GetType();
            FormatClassAttibutes fca = null;

            if (FormatClassAttibutesDiy.ContainsKey(otype))
            {
                fca = FormatClassAttibutesDiy[otype];
            }
            else
            {
#if !COREFX
                Attribute[] Attributes = Attribute.GetCustomAttributes(otype);
#else
                Attribute[] Attributes = (Attribute[])System.Linq.Enumerable.ToArray(otype.GetTypeInfo().GetCustomAttributes(false));
#endif
                foreach (Attribute p in Attributes)
                {
                    fca = p as FormatClassAttibutes;

                    if (fca != null)
                    {
                        FormatClassAttibutesDiy.Add(otype, fca);
                        break;
                    }
                }
            }
            if (fca != null)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryWriter bufflist = new BinaryWriter(stream);

                    bufflist.Write(GetBytes(fca.BufferCmdType));

                    byte[] classdata = SerializeObject(o);
                    bufflist.Write(GetBytes(classdata.Length));
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

                    byte[] tmp = GetBytes(x);

                    int l = fdata.Length + tmp.Length;

                    byte[] data = GetBytes(l);

                    bufflist.Write((byte)0xFF);
                    bufflist.Write(data);
                    bufflist.Write(fdata);

                    byte[] pdata = stream.ToArray();
#if !COREFX
                    stream.Close();
#endif
                    stream.Dispose();
                    return pdata;
                }
            }

#if !COREFX
            throw new EntryPointNotFoundException("无法找到 FormatClassAttibutes 标签");
#else
            throw new ArgumentException("无法找到 FormatClassAttibutes 标签");
#endif

        }

        public static byte[] GetBytes(int data)
        {
            List<byte> pdata = new List<byte>();

            for (; ; )
            {
                if ((data & ~127) == 0)
                {
                    pdata.Add((byte)data);
                    return pdata.ToArray();
                }
                pdata.Add((byte)(data & 127 | 128));
                data = data >> 7;
            }          
        }
    }
}

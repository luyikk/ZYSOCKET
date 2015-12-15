using System;
using System.Collections.Generic;
using System.Text;

namespace ZYSocket.share
{
    public class BufferFormatV2:BufferFormat
    {
        public BufferFormatV2(int buffType, FDataExtraHandle dataExtra):base(buffType,dataExtra)
        {
            buffList.Clear();
            buffList.AddRange(GetBytes(buffType));
            Encode = Encoding.Unicode;
            finish = false;
            this.dataextra = dataExtra;       
        
        }        

        public BufferFormatV2(int buffType):base(buffType)
        {
            buffList.Clear();
            buffList.AddRange(GetBytes(buffType));
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
            buffList.AddRange(GetBytes(data.Length));
            buffList.AddRange(data);
        }



        public override void AddItem(string data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            Byte[] bytes = Encode.GetBytes(data);
            buffList.AddRange(GetBytes(bytes.Length));
            buffList.AddRange(bytes);
        }


        public override void AddItem(byte[] data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            byte[] ldata = GetBytes(data.Length);
            buffList.AddRange(ldata);
            buffList.AddRange(data);
        }

        public override byte[] Finish()
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");


            if (dataextra != null)
            {
                byte[] fdata = dataextra(buffList.ToArray());
                buffList.Clear();
                buffList.AddRange(fdata);
            }


         

            int x = buffList.Count;

            if ((buffList.Count+1)<128)
            {
                x += 1;
            }
            else if ((buffList.Count + 2) < 16384)
            {
                x += 2;
            }
            else if ((buffList.Count + 3) < 2097152)
            {
                x += 3;
            }
            else
            {
                x += 4;
            }
           

            byte[] tmp = GetBytes(x);

            int l = buffList.Count + tmp.Length;

            byte[] data = GetBytes(l);

            for (int i = data.Length - 1; i >= 0; i--)
            {
                buffList.Insert(0, data[i]);
            }
            buffList.Insert(0, 0xff);

            byte[] datap = new byte[buffList.Count];

            buffList.CopyTo(0, datap, 0, datap.Length);

            buffList.Clear();
            finish = true;

            return datap;
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
            Attribute[] Attributes = Attribute.GetCustomAttributes(otype);

            foreach (Attribute p in Attributes)
            {
                FormatClassAttibutes fca = p as FormatClassAttibutes;

                if (fca != null)
                {
                    List<byte> bufflist = new List<byte>();
                  
                    bufflist.AddRange(GetBytes(fca.BufferCmdType));

                    byte[] classdata = SerializeObject(o);
                    bufflist.AddRange(GetBytes(classdata.Length));
                    bufflist.AddRange(classdata);

                    if (dataExtra != null)
                    {
                        byte[] fdata = dataExtra(bufflist.ToArray());
                        bufflist.Clear();
                        bufflist.AddRange(fdata);
                    }


                    int x = bufflist.Count;

                    if ((bufflist.Count + 1) < 128)
                    {
                        x += 1;
                    }
                    else if ((bufflist.Count + 2) < 16384)
                    {
                        x += 2;
                    }
                    else if ((bufflist.Count + 3) < 2097152)
                    {
                        x += 3;
                    }
                    else
                    {
                        x += 4;
                    }                  

                    byte[] tmp = GetBytes(x);

                    int l = bufflist.Count + tmp.Length;

                    byte[] data = GetBytes(l);

                    for (int i = data.Length - 1; i >= 0; i--)
                    {
                        bufflist.Insert(0, data[i]);
                    }

                    bufflist.Insert(0, 0xff);

                    byte[] datap = new byte[bufflist.Count];

                    bufflist.CopyTo(0, datap, 0, datap.Length);

                    bufflist.Clear();

                    return datap;
                }
            }

            throw new EntryPointNotFoundException("无法找到 FormatClassAttibutes 标签");
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

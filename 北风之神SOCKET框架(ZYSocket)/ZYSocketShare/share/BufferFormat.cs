/*
 * 北风之神SOCKET框架(ZYSocket)
 *  Borey Socket Frame(ZYSocket)
 *  by luyikk@126.com QQ:547386448
 *  Updated 2012-07-18 
 */
using System;
using System.Collections.Generic;
using System.Text;
#if !COREFX
using System.Runtime.Serialization.Formatters.Binary;
#endif
using System.Xml;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace ZYSocket.share
{

    public enum BuffFormatType
    {
#if !COREFX
        XML = 0,
        Binary=1,
#endif
     
#if Net4
        MsgPack=4,

#endif
        protobuf = 5,

    }

    /// <summary>
    /// 数据包在格式化完毕后回调方法。（例如加密，压缩等）
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public delegate byte[] FDataExtraHandle(byte[] data);

    /// <summary>
    /// 数据包格式化类
    /// (此类功能是讲.NET数据转换成通讯数据包）
    /// </summary>
    public class BufferFormat
    {

        /// <summary>
        /// 对象格式化方式
        /// </summary>
        public static BuffFormatType ObjFormatType { get; set; }

        public static Dictionary<Type, FormatClassAttibutes> FormatClassAttibutesDiy { get; set; }

        static BufferFormat()
        {       

            ObjFormatType = BuffFormatType.protobuf;
            FormatClassAttibutesDiy = new Dictionary<Type, FormatClassAttibutes>();

        }


        protected MemoryStream stream;
        protected System.IO.BinaryWriter buffList;

        /// <summary>
        /// 字符串格式化字符编码
        /// </summary>
        public static Encoding Encode { get; set; } =  Encoding.UTF8;

        protected FDataExtraHandle dataextra;

        protected bool finish;
        /// <summary>
        /// 数据包格式化类
        /// </summary>
        /// <param name="buffType">包类型</param>
        /// <param name="dataExtra">数据包在格式化完毕后回调方法。（例如加密，压缩等）</param>
        public BufferFormat(int buffType,FDataExtraHandle dataExtra)
        {
            stream = new MemoryStream();
            buffList = new BinaryWriter(stream);

            buffList.Write(0);
            buffList.Write(GetSocketBytes(buffType));
           
            finish = false;
            this.dataextra=dataExtra;
        }


        /// <summary>
        /// 数据包格式化类
        /// </summary>
        /// <param name="buffType">包类型</param>
        public BufferFormat(int buffType)
        {
            stream = new MemoryStream();
            buffList = new BinaryWriter(stream);
            buffList.Write(0);
            buffList.Write(GetSocketBytes(buffType));
         
            finish = false;
        }





#region 布尔值
        /// <summary>
        /// 添加一个布尔值
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(bool data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.Write(data);
        }

#endregion

#region 整数
        /// <summary>
        /// 添加一个1字节的整数
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(byte data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

             buffList.Write(data);
        }

        /// <summary>
        /// 添加一个2字节的整数
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(Int16 data)
        {
            buffList.Write(data);
        }

        /// <summary>
        /// 添加一个2字节的整数
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(UInt16 data)
        {
            buffList.Write(data);
        }

        /// <summary>
        /// 添加一个4字节的整数
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(Int32 data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.Write(data);
        }


        /// <summary>
        /// 添加一个4字节的整数
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(UInt32 data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.Write(data);
        }

        /// <summary>
        /// 添加一个8字节的整数
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(Int64 data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.Write(data);
        }

        /// <summary>
        /// 添加一个8字节的整数
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(UInt64 data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.Write(data);
        }

#endregion

#region 浮点数

        /// <summary>
        /// 添加一个4字节的浮点
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(float data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.Write(data);
        }

        /// <summary>
        /// 添加一个8字节的浮点
        /// </summary>
        /// <param name="data"></param>
        public void AddItem(double data)
        {
            buffList.Write(data);
        }

#endregion

#region 数据包

        /// <summary>
        /// 添加一个BYTE[]数据包
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual void AddItem(Byte[] data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");
                      
            buffList.Write(data.Length);
            buffList.Write(data);

        }

#endregion

#region 字符串
        /// <summary>
        /// 添加一个字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual void AddItem(String data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            Byte[] bytes = Encode.GetBytes(data);
            buffList.Write(bytes.Length);
            buffList.Write(bytes);

        }

#endregion

#region 时间
        /// <summary>
        /// 添加一个一个DATATIME
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual void AddItem(DateTime data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            AddItem(data.ToString());
        }

#endregion

#region 对象
        /// <summary>
        /// 将一个对象转换为二进制数据
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual void AddItem(object obj)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            byte[] data = SerializeObject(obj);
            buffList.Write(data.Length);
            buffList.Write(data);
        }

#endregion


       

        /// <summary>
        /// 直接格式化一个带FormatClassAttibutes 标签的类，返回BYTE[]数组，此数组可以直接发送不需要组合所数据包。所以也称为类抽象数据包
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public  static byte[] FormatFCA(object o)
        {
            return FormatFCA(o, null);
        }

       /// <summary>
        /// 直接格式化一个带FormatClassAttibutes 标签的类，返回BYTE[]数组，此数组可以直接发送不需要组合所数据包。所以也称为类抽象数据包
       /// </summary>
       /// <param name="o"></param>
       /// <param name="dataExtra">数据加密回调</param>
       /// <returns></returns>
        public  static byte[] FormatFCA(object o, FDataExtraHandle dataExtra)
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
                Attribute[] Attributes = System.Linq.Enumerable.ToArray(otype.GetTypeInfo().GetCustomAttributes(false));
#endif

                foreach (Attribute p in Attributes)
                {
                    fca = p as FormatClassAttibutes;

                    if (fca != null)
                    {
                        FormatClassAttibutesDiy[otype]=fca;
                        break;
                    }
                }
            }

            if (fca != null)
            {
                using (MemoryStream stream = new MemoryStream())
                {

                    BinaryWriter bufflist = new BinaryWriter(stream);


                    if (dataExtra != null)
                    {

                        bufflist.Write(fca.BufferCmdType);
                        byte[] classdata = SerializeObject(o);
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
                        bufflist.Write(fca.BufferCmdType);
                        byte[] classdata = SerializeObject(o);
                        bufflist.Write(classdata.Length);
                        bufflist.Write(classdata);

                    }


                    int l = (int)(stream.Length);

                    byte[] data = GetSocketBytes(l);

                    stream.Position = 0;

                    bufflist.Write(data);


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


        /// <summary>
        /// 完毕
        /// </summary>
        /// <returns></returns>
        public virtual byte[] Finish()
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");


            if (dataextra != null)
            {
                byte[] fdata = dataextra(stream.ToArray());
                stream.Position = 0;
                stream.SetLength(0);               
                buffList.Write(fdata);
            }


            int l = (int)(stream.Length);

            byte[] data = GetSocketBytes(l);

            stream.Position = 0;

            buffList.Write(data);
                      

            byte[] pdata= stream.ToArray();
#if !COREFX
            stream.Close();
#endif
            stream.Dispose();

            finish = true;

            return pdata;
        }



#region V对象
        ///// <summary>
        ///// 把对象序列化并返回相应的字节
        ///// </summary>
        ///// <param name="pObj">需要序列化的对象</param>
        ///// <returns>byte[]</returns>
        //public  static byte[] SerializeObject(object pObj)
        //{
        //    System.IO.MemoryStream _memory = new System.IO.MemoryStream();
        //    BinaryFormatter formatter = new BinaryFormatter();
        //    // formatter.TypeFormat=System.Runtime.Serialization.Formatters.FormatterTypeStyle.XsdString;
        //    formatter.Serialize(_memory, pObj);
        //    _memory.Position = 0;
        //    byte[] read = new byte[_memory.Length];
        //    _memory.Read(read, 0, read.Length);
        //    _memory.Close();
        //    return read;
        //}


        /// <summary>
        /// 把对象序列化并返回相应的字节
        /// </summary>
        /// <param name="pObj">需要序列化的对象</param>
        /// <returns>byte[]</returns>
        public static byte[] SerializeObject(object pObj)
        {

            //StringBuilder sBuilder = new StringBuilder();

            //XmlSerializer xmlSerializer = new XmlSerializer(pObj.GetType());
            //XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            //xmlWriterSettings.Encoding = Encoding.Unicode;
            //XmlWriter xmlWriter = XmlWriter.Create(sBuilder, xmlWriterSettings);
            //xmlSerializer.Serialize(xmlWriter, pObj);
            //xmlWriter.Close();

            //return Encoding.UTF8.GetBytes(sBuilder.ToString());

            switch (ObjFormatType)
            {
#if !COREFX
                case BuffFormatType.Binary:
                    {
                        using (System.IO.MemoryStream _memory = new System.IO.MemoryStream())
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            // formatter.TypeFormat=System.Runtime.Serialization.Formatters.FormatterTypeStyle.XsdString;
                            formatter.Serialize(_memory, pObj);
                            //_memory.Position = 0;
                            //byte[] read = new byte[_memory.Length];
                            //_memory.Read(read, 0, read.Length);
                            //_memory.Close();                            
                            //return read;

                            return _memory.ToArray();
                        }
                    }
                case BuffFormatType.XML:
                    {
                        StringBuilder sBuilder = new StringBuilder();

                        XmlSerializer xmlSerializer = new XmlSerializer(pObj.GetType());
                        XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                        xmlWriterSettings.Encoding = Encoding.Unicode;
                        XmlWriter xmlWriter = XmlWriter.Create(sBuilder, xmlWriterSettings);
                        xmlSerializer.Serialize(xmlWriter, pObj);
                        xmlWriter.Close();

                        return Encoding.UTF8.GetBytes(sBuilder.ToString());
                    }     
#endif
#if Net4
                case BuffFormatType.MsgPack:
                    {
                        return MsgPack.Serialization.SerializationContext.Default.GetSerializer(pObj.GetType()).PackSingleObject(pObj);
                    }
             
#endif
                case BuffFormatType.protobuf:
                    {
                        using (System.IO.MemoryStream _memory = new System.IO.MemoryStream())
                        {
                            ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(_memory, pObj);

                            return _memory.ToArray();

                        }
                    }
                default:
                    {
                        using (System.IO.MemoryStream _memory = new System.IO.MemoryStream())
                        {
                            ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(_memory, pObj);

                            return _memory.ToArray();

                        }
                    }
            }




        }




#endregion

#region V整数
        /// <summary>
        /// 将一个32位整形转换成一个BYTE[]4字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Byte[] GetSocketBytes(Int32 data)
        {
            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// 将一个32位整形转换成一个BYTE[]4字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Byte[] GetSocketBytes(UInt32 data)
        {
            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// 将一个64位整形转换成一个BYTE[]8字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  static Byte[] GetSocketBytes(UInt64 data)
        {
            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// 将一个64位整形转换成一个BYTE[]8字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Byte[] GetSocketBytes(Int64 data)
        {
            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// 将一个 1位CHAR转换成1位的BYTE[]
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  static  Byte[] GetSocketBytes(Char data)
        {
            Byte[] bytes = new Byte[] { (Byte)data };
            return bytes;
        }

        /// <summary>
        /// 将一个 16位整数转换成2位的BYTE[]
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  static Byte[] GetSocketBytes(Int16 data)
        {
            return BitConverter.GetBytes(data);
        }


        /// <summary>
        /// 将一个 16位整数转换成2位的BYTE[]
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Byte[] GetSocketBytes(UInt16 data)
        {
            return BitConverter.GetBytes(data);
        }


#endregion

#region V布尔值
        /// <summary>
        /// 将一个布尔值转换成一个BYTE[]
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  static Byte[] GetSocketBytes(bool data)
        {
            return BitConverter.GetBytes(data);
        }
#endregion

#region V浮点数
        /// <summary>
        /// 将一个32位浮点数转换成一个BYTE[]4字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  static Byte[] GetSocketBytes(float data)
        {
            return BitConverter.GetBytes(data);
        }

        /// <summary>
        /// 将一个64位浮点数转换成一个BYTE[]8字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  static Byte[] GetSocketBytes(double data)
        {
            return BitConverter.GetBytes(data);
        }
#endregion


    }

}

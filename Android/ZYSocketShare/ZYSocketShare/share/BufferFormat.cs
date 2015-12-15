/*
 * 北风之神SOCKET框架(ZYSocket)
 *  Borey Socket Frame(ZYSocket)
 *  by luyikk@126.com QQ:547386448
 *  Updated 2012-07-18 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;

namespace ZYSocket.share
{

    public enum BuffFormatType
    {
        XML=0,
        Binary=1
      

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

        static BufferFormat()
        {
            ObjFormatType = BuffFormatType.XML;
        }


        protected List<byte> buffList;

        /// <summary>
        /// 字符串格式化字符编码
        /// </summary>
        public Encoding Encode { get; set; }

        protected FDataExtraHandle dataextra;

        protected bool finish;
        /// <summary>
        /// 数据包格式化类
        /// </summary>
        /// <param name="buffType">包类型</param>
        /// <param name="dataExtra">数据包在格式化完毕后回调方法。（例如加密，压缩等）</param>
        public BufferFormat(int buffType,FDataExtraHandle dataExtra)
        {
            
            buffList = new List<byte>();
            buffList.AddRange(GetSocketBytes(buffType));
            Encode = Encoding.Unicode;
            finish = false;
            this.dataextra=dataExtra;
        }


        /// <summary>
        /// 数据包格式化类
        /// </summary>
        /// <param name="buffType">包类型</param>
        public BufferFormat(int buffType)
        {
           
            buffList = new List<byte>();
            buffList.AddRange(GetSocketBytes(buffType));
            Encode = Encoding.Unicode;
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

            buffList.AddRange(GetSocketBytes(data));
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

            buffList.Add(data);
        }

        /// <summary>
        /// 添加一个2字节的整数
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(Int16 data)
        {
            buffList.AddRange(GetSocketBytes(data));
        }

        /// <summary>
        /// 添加一个2字节的整数
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(UInt16 data)
        {
            buffList.AddRange(GetSocketBytes(data));
        }

        /// <summary>
        /// 添加一个4字节的整数
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(Int32 data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.AddRange(GetSocketBytes(data));
        }


        /// <summary>
        /// 添加一个4字节的整数
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(UInt32 data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.AddRange(GetSocketBytes(data));
        }

        /// <summary>
        /// 添加一个8字节的整数
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(Int64 data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.AddRange(GetSocketBytes(data));
        }

        /// <summary>
        /// 添加一个8字节的整数
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddItem(UInt64 data)
        {
            if (finish)
                throw new ObjectDisposedException("BufferFormat", "无法使用已经调用了 Finish 方法的BufferFormat对象");

            buffList.AddRange(GetSocketBytes(data));
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

            buffList.AddRange(GetSocketBytes(data));
        }

        /// <summary>
        /// 添加一个8字节的浮点
        /// </summary>
        /// <param name="data"></param>
        public void AddItem(double data)
        {
            buffList.AddRange(GetSocketBytes(data));
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

            byte[] ldata = GetSocketBytes(data.Length);
            buffList.AddRange(ldata);
            buffList.AddRange(data);

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
            buffList.AddRange(GetSocketBytes(bytes.Length));
            buffList.AddRange(bytes);

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
            buffList.AddRange(GetSocketBytes(data.Length));
            buffList.AddRange(data);
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
            Attribute[] Attributes = Attribute.GetCustomAttributes(otype);

            foreach (Attribute p in Attributes)
            {
                FormatClassAttibutes fca = p as FormatClassAttibutes;

                if (fca != null)
                {
                    List<byte> bufflist = new List<byte>();

                    bufflist.AddRange(GetSocketBytes(fca.BufferCmdType));

                    byte[] classdata = SerializeObject(o);
                    bufflist.AddRange(GetSocketBytes(classdata.Length));
                    bufflist.AddRange(classdata);

                    if (dataExtra != null)
                    {
                        byte[] fdata = dataExtra(bufflist.ToArray());
                        bufflist.Clear();
                        bufflist.AddRange(fdata);
                    }


                    int l = bufflist.Count + 4;
                    byte[] data = GetSocketBytes(l);
                    for (int i = data.Length - 1; i >= 0; i--)
                    {
                        bufflist.Insert(0, data[i]);
                    }

                    byte[] datap = new byte[bufflist.Count];

                    bufflist.CopyTo(0, datap, 0, datap.Length);

                    bufflist.Clear();

                    return datap;
                }
            }

            throw new EntryPointNotFoundException("无法找到 FormatClassAttibutes 标签");
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
                byte[] fdata = dataextra(buffList.ToArray());
                buffList.Clear();
                buffList.AddRange(fdata);
            }


            int l = buffList.Count + 4;

            byte[] data = GetSocketBytes(l);

            for (int i = data.Length - 1; i >= 0; i--)
            {
                buffList.Insert(0, data[i]);
            }

            byte[] datap = new byte[buffList.Count];

            buffList.CopyTo(0, datap, 0, datap.Length);

            buffList.Clear();
            finish = true;

            return datap;
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
                case BuffFormatType.Binary:
                    {
                        System.IO.MemoryStream _memory = new System.IO.MemoryStream();
                        BinaryFormatter formatter = new BinaryFormatter();
                        // formatter.TypeFormat=System.Runtime.Serialization.Formatters.FormatterTypeStyle.XsdString;
                        formatter.Serialize(_memory, pObj);
                        _memory.Position = 0;
                        byte[] read = new byte[_memory.Length];
                        _memory.Read(read, 0, read.Length);
                        _memory.Close();
                        return read;
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
                default:
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

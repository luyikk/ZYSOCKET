using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;
namespace ZYSocket.RPC
{
    public static class Serialization
    {


    

        static Serialization()
        {
           
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



        public static object UnpackSingleObject(Type type, byte[] data)
        {

            if (type == typeof(int))
            {
                return ReadInt32(data);
            }
            else if (type == typeof(uint))
            {
                return ReadUInt32(data);
            }
            else if (type == typeof(byte))
            {
                return ReadByte(data);
            }
            else if (type == typeof(short))
            {
                return ReadInt16(data);
            }
            else if (type == typeof(ushort))
            {
                return ReadUint16(data);
            }
            else if (type == typeof(long))
            {
                return ReadInt64(data);
            }
            else if (type == typeof(ulong))
            {
                return ReadUInt64(data);
            }
            else if (type == typeof(bool))
            {
                return ReadBoolean(data);
            }
            else if (type == typeof(float))
            {
                return ReadFloat(data);
            }
            else if (type == typeof(double))
            {
                return ReadDouble(data);
            }
            else if (type == typeof(string))
            {
                return ReadString(data);
            }
            else if (type == typeof(byte[]))
            {
                return data;
            }
            else
                return ProtoUnpackSingleObject(type,data);


        }

        public static byte[] PackSingleObject(Type type, object obj)
        {

            if (type == typeof(int))
            {
                return BitConverter.GetBytes((int)obj);
            }
            else if (type == typeof(uint))
            {
                return BitConverter.GetBytes((uint)obj);
            }
            else if (type == typeof(byte))
            {
                return BitConverter.GetBytes((byte)obj);
            }
            else if (type == typeof(short))
            {
                return BitConverter.GetBytes((short)obj);
            }
            else if (type == typeof(ushort))
            {
                return BitConverter.GetBytes((ushort)obj);
            }
            else if (type == typeof(long))
            {
                return BitConverter.GetBytes((long)obj);
            }
            else if (type == typeof(ulong))
            {
                return BitConverter.GetBytes((ulong)obj);
            }
            else if (type == typeof(bool))
            {
                return BitConverter.GetBytes((bool)obj);
            }
            else if (type == typeof(float))
            {
                return BitConverter.GetBytes((float)obj);
            }
            else if (type == typeof(double))
            {
                return BitConverter.GetBytes((double)obj);
            }
            else if (type == typeof(string))
            {
                return Encoding.UTF8.GetBytes((string)obj);
            }
            else if (type == typeof(byte[]))
            {
                return (byte[])obj;
            }
            else
                return ProtoBufPackSingleObject(obj);


        }


        #region return 整数
        /// <summary>
        /// 读取内存流中的头2位并转换成整型
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static short ReadInt16(byte[] Data)
        {
            short values = BitConverter.ToInt16(Data, 0);          
            return values;
        }

        /// <summary>
        /// 读取内存流中的头2位并转换成无符号整型
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static ushort ReadUint16(byte[] Data)
        {
            ushort values = BitConverter.ToUInt16(Data, 0);         
            return values;
        }


        /// <summary>
        /// 读取内存流中的头4位并转换成整型
        /// </summary>
        /// <param name="ms">内存流</param>
        /// <returns></returns>
        public static int ReadInt32(byte[] Data)
        {

            int values = BitConverter.ToInt32(Data, 0);         
            return values;

        }

        /// <summary>
        /// 读取内存流中的头4位并转换成无符号整型
        /// </summary>
        /// <param name="ms">内存流</param>
        /// <returns></returns>
        public static uint ReadUInt32(byte[] Data)
        {

            uint values = BitConverter.ToUInt32(Data, 0);        
            return values;

        }


        /// <summary>
        /// 读取内存流中的头8位并转换成长整型
        /// </summary>
        /// <param name="ms">内存流</param>
        /// <returns></returns>
        public static long ReadInt64(byte[] Data)
        {

            long values = BitConverter.ToInt64(Data, 0);       
            return values;

        }


        /// <summary>
        /// 读取内存流中的头8位并转换成无符号长整型
        /// </summary>
        /// <param name="ms">内存流</param>
        /// <returns></returns>
        public static ulong ReadUInt64(byte[] Data)
        {

            ulong values = BitConverter.ToUInt64(Data, 0);          
            return values;

        }

        /// <summary>
        /// 读取内存流中的首位
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static byte ReadByte(byte[] Data)
        {

            byte values = (byte)Data[0];         
            return values;

        }

        #endregion

        #region return 布尔值
        /// <summary>
        /// 读取内存流中的头1位并转换成布尔值
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static bool ReadBoolean(byte[] Data)
        {

            bool values = BitConverter.ToBoolean(Data, 0);        
            return values;

        }

        #endregion

        #region return 浮点数


        /// <summary>
        /// 读取内存流中的头4位并转换成单精度浮点数
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static float ReadFloat(byte[] Data)
        {
            float values = BitConverter.ToSingle(Data, 0);         
            return values;
        }


        /// <summary>
        /// 读取内存流中的头8位并转换成浮点数
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static double ReadDouble(byte[] Data)
        {

            double values = BitConverter.ToDouble(Data, 0);      
            return values;

        }


        #endregion

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
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ZYSocket.RPCX.Client
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

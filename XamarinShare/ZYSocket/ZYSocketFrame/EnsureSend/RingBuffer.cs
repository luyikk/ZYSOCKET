using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYSocket.EnsureSend
{
    public class RingBuffer
    {
        protected object lockobj = new object();
        /// <summary>
        /// 最大缓冲数
        /// </summary>
        protected int MAXSIZE;

        /// <summary>
        /// 当前数据环
        /// </summary>
        public byte[] Data { get; protected set; }


        protected int _current;

        /// <summary>
        /// 当前游标
        /// </summary>
        public int Current
        {
            get { return _current; }
            private set
            {
                _current = value;
            }
        }


        protected int _length;
        /// <summary>
        /// 当前数据长度
        /// </summary>
        public int Length { get { return _length; } protected set { _length = value; } }


        public RingBuffer(int maxSize)
        {
            this.MAXSIZE = maxSize;
            Data = new byte[maxSize];
        }

        

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public bool Write(byte[] data, int offset, int count)
        {

            System.Threading.Monitor.Enter(lockobj);

            try
            {

                if (offset < 0)
                {
                    throw new ArgumentOutOfRangeException("offset", "偏移量不能小于0");
                }
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException("count", "写入数量不能小于0");
                }


                int lengt = count;

                if (lengt > data.Length)
                    lengt = data.Length;


                if (lengt > MAXSIZE)
                {
#if DEBUG
                    throw new Exception("写入的数据包长度超出环总长度");
#else
                    return false;
#endif

                }


                if (Length + lengt > MAXSIZE)
                {
                    return false;
                }



                int savelen, savepos;           // 数据要保存的长度和位置
                if (_current + _length < MAXSIZE)
                {   // INBUF中的剩余空间有回绕
                    savelen = MAXSIZE - (_current + _length);        // 后部空间长度，最大接收数据的长度
                }
                else
                {
                    savelen = MAXSIZE - _length;
                }

                if (savelen > lengt)
                    savelen = lengt;


                // 缓冲区数据的末尾
                savepos = (_current + _length) % MAXSIZE;

                Buffer.BlockCopy(data, offset, Data, savepos, savelen);

                Length += savelen;

                int have = lengt - savelen;
                if (have > 0)
                {
                    savepos = (_current + Length) % MAXSIZE;
                    Buffer.BlockCopy(data, offset + (lengt - have), Data, savepos, have);
                    Length += have;
                }

                return true;

            }
            finally
            {
                System.Threading.Monitor.Exit(lockobj);
            }

        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="data"></param>
        public bool Write(byte[] data)
        {

            System.Threading.Monitor.Enter(lockobj);

            try
            {

                if (data.Length > MAXSIZE)
                {
#if DEBUG
                    throw new Exception("写入的数据包长度超出环总长度");
#else
                    return false;
#endif

                }


                if (Length + data.Length > MAXSIZE)
                {
                    return false;
                }



                int savelen, savepos;           // 数据要保存的长度和位置
                if (_current + _length < MAXSIZE)
                {   // INBUF中的剩余空间有回绕
                    savelen = MAXSIZE - (_current + _length);        // 后部空间长度，最大接收数据的长度
                }
                else
                {
                    savelen = MAXSIZE - _length;
                }

                if (savelen > data.Length)
                    savelen = data.Length;


                // 缓冲区数据的末尾
                savepos = (_current + _length) % MAXSIZE;

                Buffer.BlockCopy(data, 0, Data, savepos, savelen);

                Length += savelen;

                int have = data.Length - savelen;
                if (have > 0)
                {
                    savepos = (_current + Length) % MAXSIZE;
                    Buffer.BlockCopy(data, data.Length - have, Data, savepos, have);
                    Length += have;
                }

                return true;

            }
            finally
            {
                System.Threading.Monitor.Exit(lockobj);
            }

        }

        
        public byte[] Read(int lengt)
        {
            System.Threading.Monitor.Enter(lockobj);
            try
            {

                if (lengt > MAXSIZE)
                {
#if DEBUG
                    throw new Exception("读取的数据包长度数超出环总长度");
#else
                    return null;
#endif

                }


                if (lengt > Length)
                {
#if DEBUG
                    throw new Exception("没有那么多数据可读取");
#else
                    return null;
#endif
                }

                if (lengt < 0)
                {
                    return null;
                }


                byte[] data = new byte[lengt];

                // 复制出一个消息
                if (_current + lengt > MAXSIZE)
                {
                    // 如果一个消息有回卷（被拆成两份在环形缓冲区的头尾）
                    // 先拷贝环形缓冲区末尾的数据
                    int copylen = MAXSIZE - _current;

                    Buffer.BlockCopy(Data, _current, data, 0, copylen);

                    // 再拷贝环形缓冲区头部的剩余部分              
                    Buffer.BlockCopy(Data, 0, data, copylen, lengt - copylen);

                }
                else
                {
                    // 消息没有回卷，可以一次拷贝出去

                    if (lengt < 8) //小于8 使用whlie COPY
                    {
                        int num2 = lengt;
                        while (--num2 >= 0)
                        {
                            data[num2] = this.Data[this._current + num2];
                        }
                    }
                    else
                    {
                        Buffer.BlockCopy(Data, _current, data, 0, lengt);
                    }
                }

                // 重新计算环形缓冲区头部位置
                Current = (_current + lengt) % MAXSIZE;
                Length -= lengt;

                return data;
            }
            finally
            {
                System.Threading.Monitor.Exit(lockobj);
            }

        }

        /// <summary>
        /// 读取指定数量的BYTES 不会设置 POSTION 和LENGTH
        /// </summary>
        /// <param name="lengt"></param>
        /// <returns></returns>
        public byte[] ReadNoPostion(int lengt)
        {
            System.Threading.Monitor.Enter(lockobj);

            try
            {
                if (lengt > MAXSIZE)
                {
#if DEBUG
                    throw new Exception("读取的数据包长度数超出环总长度");
#else
                    return null;
#endif

                }

                if (lengt > Length)
                {
                    return null;
                }

                if (lengt < 0)
                {
                    return null;
                }

                byte[] data = new byte[lengt];

                // 复制出一个消息
                if (_current + lengt > MAXSIZE)
                {
                    // 如果一个消息有回卷（被拆成两份在环形缓冲区的头尾）
                    // 先拷贝环形缓冲区末尾的数据
                    int copylen = MAXSIZE - _current;

                    Buffer.BlockCopy(Data, _current, data, 0, copylen);

                    // 再拷贝环形缓冲区头部的剩余部分              
                    Buffer.BlockCopy(Data, 0, data, copylen, lengt - copylen);

                }
                else
                {
                    // 消息没有回卷，可以一次拷贝出去

                    if (lengt < 8) //小于8 使用whlie COPY
                    {
                        int num2 = lengt;
                        while (--num2 >= 0)
                        {
                            data[num2] = this.Data[this._current + num2];
                        }
                    }
                    else
                    {
                        Buffer.BlockCopy(Data, _current, data, 0, lengt);
                    }
                }

                return data;

            }
            finally
            {
                System.Threading.Monitor.Exit(lockobj);
            }


        }

        public void SubLength(int value)
        {
            System.Threading.Monitor.Enter(lockobj);

            try
            {
                // 重新计算环形缓冲区头部位置
                Current = (_current + value) % MAXSIZE;
                Length -= value;
            }
            finally
            {
                System.Threading.Monitor.Exit(lockobj);
            }
        }

    }
}

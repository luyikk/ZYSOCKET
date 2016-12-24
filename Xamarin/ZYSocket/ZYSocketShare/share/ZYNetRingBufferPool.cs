using System;
using System.Collections.Generic;
using System.Text;


namespace ZYSocket.share
{
    /// <summary>
    /// 环形数据包缓冲区
    /// </summary>
    public class ZYNetRingBufferPool
    {

        

        protected object lockobj = new object();
        /// <summary>
        /// 最大缓冲数
        /// </summary>
        protected int MAXSIZE;

        /// <summary>
        /// 读取数据包头大小
        /// </summary>
        protected byte Hib;


        /// <summary>
        /// 是否大码
        /// </summary>
        protected bool IsBigEncoding = false;

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


        public ZYNetRingBufferPool():this(1024*1024,4,false)
        {

        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="MaxSize">内存环大小</param>
        public ZYNetRingBufferPool(int MaxSize):this(MaxSize,4,false)
        {

        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="isBig">是否大码(用于linux)</param>
        public ZYNetRingBufferPool(bool isBig)
            : this(1024 * 1024, 4, isBig)
        {

        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="MaxSize">内存环大小</param>
        /// <param name="hiB">数据包长度占用位数</param>
        public ZYNetRingBufferPool(int MaxSize, byte hiB):this(MaxSize,hiB,false)
        {

        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="MaxSize">内存环大小</param>
        /// <param name="hiB">数据包长度占用位数</param>
        /// <param name="IsBig">大码还是小码</param>
        public ZYNetRingBufferPool(int MaxSize, byte hiB, bool IsBig)
        {
            Hib = hiB;
            IsBigEncoding = IsBig;
            MAXSIZE = MaxSize;
            Data = new byte[MaxSize];
        }


         /// <summary>
         /// 写入数据
         /// </summary>
         /// <param name="data">数据</param>
         /// <param name="offset">偏移</param>
         /// <param name="count">数量</param>
         /// <returns></returns>
        public bool Write(byte[] data,int offset,int count)
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
                    Buffer.BlockCopy(data, offset+(lengt - have), Data, savepos, have);
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

        /// <summary>
        /// 计算长度
        /// </summary>
        /// <returns></returns>
        protected virtual int GetHeadLengt()
        {
            if (Length < Hib)
            {               
                return 0;
            }
                      

            int res = 0;

            if (IsBigEncoding)
            {
                for (int i = 0, l = Hib - 1; i < Hib; i++, l--)
                {
                    int temp = ((int)Data[(_current + i) % MAXSIZE]) & 0xff;
                    temp <<= l * 8;
                    res = temp + res;
                }
            }
            else
            {
                for (int i = 0; i < Hib; i++)
                {
                    int temp = ((int)Data[(_current+i)%MAXSIZE]) & 0xff;
                    temp <<= i * 8;
                    res = temp + res;
                }
            }

            return res;
        }


        public virtual void Flush()
        {
            System.Threading.Monitor.Enter(lockobj);
            try
            {

                Current = 0;
                _length = 0;
            }
            finally
            {
                System.Threading.Monitor.Exit(lockobj);
            }
        }


        public virtual bool Read(out byte[] data)
        {
            int count = GetHeadLengt();

            if (count == 0)
            {
                data = null;
                return false;
            }

            if (count > MAXSIZE)
            {
                Flush();
                data = null;
                return false;
            }

            if (count > _length)
            {
                data = null;
                return false;
            }

            if (count < 0)
            {
                this.Flush();
                data = null;
                return false;
            }


            data= Read(count);

            return true;
        }
    }
}

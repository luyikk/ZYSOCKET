/*
 * 北风之神SOCKET框架(ZYSocket)
 *  Borey Socket Frame(ZYSocket)
 *  by luyikk@126.com QQ:547386448
 *  Updated 2012-07-18 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ZYSocket.share
{
    public class ZYNetBufferReadStream:Stream
    {

        protected byte[] Datas { get; set; }

        protected int _capacity;
        protected int _length;
        protected bool _canRead = false;
        protected bool _canWrile = false;
        protected int _position;
        protected int _headlengt;
        protected int _pw;
        protected int _SpLengt;

        protected override void Dispose(bool disposing)
        {
            _canRead = false;
            _canWrile = false;
          
            base.Dispose(disposing);
        }
        protected void thowStreamClose()
        {
            throw new ObjectDisposedException(null, "流已经关闭了");
        }

        public virtual int Capacity
        {
            get
            {
                return _capacity;
            }
            set
            {
                if (!this._canRead)
                {
                    thowStreamClose();
                }


                if (value != this._capacity)
                {

                    if (value < this._length)
                    {
                        throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_SmallCapacity");                       
                    }
                    else
                    {
                        if (value > 0)
                        {
                            byte[] dst = new byte[value];
                            if (this._length > 0)
                            {
                                Buffer.BlockCopy(Datas, 0, dst, 0, this._length);
                            }
                            this.Datas = dst;
                        }
                        else
                        {
                            _canRead = false;
                            this.Datas = null;
                        }
                        this._capacity = value;
                    }
                }

            }
        }

        public override bool CanRead
        {
            get { return _canRead; }
        }

        public override bool CanSeek
        {
            get { return _canRead; }
        }

        public override bool CanWrite
        {
            get { return _canWrile; }
        }

        public override long Length
        {
            get { return _length; }
        }


        /// <summary>
        /// 数据包最长长度
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary>
        /// 数据包头使用的位数
        /// </summary>
        public int HeadBit { get; set; }

        /// <summary>
        /// 设置或读取当前数据包头长度
        /// </summary>
        public int HeadLength
        {
            get
            {
                return _headlengt;
            }
            set
            {
                _headlengt = value;
            }
        }


        public ZYNetBufferReadStream(): this(0, 4096, 4)
        {

        }


        public ZYNetBufferReadStream(int maxSize):this(0,maxSize,4)
        {
          
        }

        public ZYNetBufferReadStream(int maxSize, int headBit) : this(0, maxSize, headBit)
        {

        }

        public ZYNetBufferReadStream(int capacity, int maxSize,int headBit)
        {
            MaxSize = maxSize;
            _capacity = capacity;
            Datas = new byte[_capacity];
            _canRead = true;
            _canWrile = true;
            HeadBit = headBit;
            _headlengt = -1;
            _position = 0;
            _SpLengt = maxSize * 64;
        }
        

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (!this._canRead)
                {
                    thowStreamClose();
                }

                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException("value", "Position 不能小于0");
                }
                if (value > 0x7fffffffL)
                {
                    throw new ArgumentOutOfRangeException("value", "Position 太大了");
                }

                this._position=(int)value;
            }
        }

        public override void Flush()
        {
            _capacity = MaxSize;
            Datas = new byte[_capacity];
            _length = 0;
            _canRead = true;
            _canWrile = true;           
            _headlengt = -1;
            _position = 0;
            _pw = 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!this._canRead)
            {
                thowStreamClose();
            }
                      
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "空的buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "偏移量不能小于0");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "写入数量不能小于0");
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("offset 设置错误");
            }
            int num = this._length - this._position;
            if (num > count)
            {
                num = count;
            }
            if (num <= 0)
            {
                return 0;
            }
            if (num <= 8)
            {
                int num2 = num;
                while (--num2 >= 0)
                {
                    buffer[offset + num2] = this.Datas[this._position + num2];
                }
            }
            else
            {
                Buffer.BlockCopy(this.Datas, this._position, buffer, offset, num);
            }
            this._position += num;
            return num;

        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!this._canRead)
            {
                thowStreamClose();
            }

            if (offset > 0x7fffffffL)
            {
                throw new ArgumentOutOfRangeException("value", "offset 太大了");
            }
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset < 0L)
                    {
                        throw new IOException("offset 不能小于0");
                    }
                    this._position = (int)offset;
                    break;

                case SeekOrigin.Current:                   
                    this._position += (int)offset;
                    break;

                case SeekOrigin.End:                   
                    this._position = this._length + ((int)offset);
                    break;
                default:
                    throw new ArgumentException("Argument_InvalidSeekOrigin");
            }
            return (long)this._position;
        }

        public override void SetLength(long value)
        {
            if (!this._canWrile)
            {
                throw new NotSupportedException("此数据流限制写入");
            }
            if (value > 0x7fffffffL)
            {
                throw new ArgumentOutOfRangeException("value", "value 太大了");
            }
            if ((value < 0L))
            {
                throw new ArgumentOutOfRangeException("value", "value 不能小于0");
            }
            int num =(int)value;
            if (!this.EnsureCapacity(num) && (num > this._length))
            {
                Array.Clear(this.Datas, this._length, num - this._length);
            }
            this._length = num;
            if (this._position > num)
            {
                this._position = num;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!this._canRead)
            {
                thowStreamClose();
            }
            if (!this._canWrile)
            {
                throw new NotSupportedException("此数据流限制写入");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "空的buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "偏移量不能小于0");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "写入数量不能小于0");
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("buffer的偏移后，总数据量小于count");
            }

            int num = this._position + count;

            if (num < 0)
            {
                throw new IOException("长度小于0");
            }
            if (num > this._length)
            {
                bool flag = this._position > this._length;
                if ((num > this._capacity) && this.EnsureCapacity(num))
                {
                    flag = false;
                }
                if (flag)
                {
                    Array.Clear(this.Datas, this._length, num - this._length);
                }
                this._length = num;
            }
            if (count <= 8)
            {
                int num2 = count;
                while (--num2 >= 0)
                {
                    this.Datas[this._position + num2] = buffer[offset + num2];
                }
            }
            else
            {
                Buffer.BlockCopy(buffer, offset, this.Datas, this._position, count);
            }

            this._position = num;          
        }


        private bool EnsureCapacity(int value)
        {
            if (value < 0)
            {
                throw new IOException("值不能小于0");
            }
            if (value <= this._capacity)
            {
                return false;
            }
            else
            {
                int num = value;
                if (num < 0x100)
                {
                    num = 0x100;
                }
                if (num < (this._capacity * 2))
                {
                    num = this._capacity * 2;
                }
                this.Capacity = num;
                return true;
            }
        }
        
        public bool IsOneRead
        {
            get
            {
                
                if (_length == 0)
                    return false;

                if (_position == 0)
                    return false;

                try
                {
                    CheckHeadLengt();
                }
                catch (ArgumentOutOfRangeException)
                {
                    this.Flush();
                }

                if (_headlengt == -1)
                    return false;


                return (_pw + _headlengt) <= _position;
            }

        }

        public void Write(byte[] data)
        {


            if (!this._canWrile)
            {
                throw new NotSupportedException("此数据流限制写入");
            }
            // CheckHeadLengt(data);

            Write(data, 0, data.Length);

        }

        protected virtual void SetSplitPostion(int postion)
        {
            if (!this._canRead)
            {
                thowStreamClose();
            }

            if (postion > _position)
                throw new ArgumentOutOfRangeException("剪裁点已经大于Postion");

            if (postion < 0)
                throw new ArgumentOutOfRangeException("postion", "剪裁点不能小于0");

            int num = Datas.Length-postion;
            

            byte[] dst = new byte[num];

            this._length = _length - postion;

            if (_position >= postion)
                this._position -= postion;

            if (this._length > 0)
            {
                Buffer.BlockCopy(Datas, postion, dst, 0, this._length);
            }
                        
            this.Datas = dst;
                
            this._capacity = num;

            _pw = 0;
        }

        protected virtual void RestPostion()
        {
            _position = 0;
            _pw = 0;
            _length = 0;
        }


    

        protected virtual void CheckHeadLengt()
        {
            if (_headlengt == -1)
            {
                int num = (_length - _pw);
                if (HeadBit > num)
                {                    
                    return;
                }

                int res = 0;

                for (int i = 0; i < HeadBit; i++)
                {
                    int temp = ((int)Datas[this._pw + i]) & 0xff;
                    temp <<= i * 8;
                    res = temp + res;
                }

                if (res > 0)
                {

                    if (res > MaxSize)
                    {
                        throw new ArgumentOutOfRangeException("数据包大于预设长度，如果你传入的数据比较大，请设置重新 maxSize 值");
                    }

                    this._headlengt = res;

                }
                else
                {
                    throw new ArgumentOutOfRangeException("数据包丢失");
                }
            }
        }

        public virtual bool Read(out byte[] data)
        {
            data = null;

            if (!this._canRead)
            {
                thowStreamClose();
            }

            if (_length == 0)
                return false;

            if (_position == 0)
                return false;

            if (_pw == _position)
                return false;


            try
            {
                CheckHeadLengt();
            }
            catch (ArgumentOutOfRangeException)
            {
                this.Flush();

            }


            if (_headlengt == -1)
                return false;


            if ((_pw+_headlengt) <= _position)
            {
                data = new byte[_headlengt];

                Buffer.BlockCopy(Datas, _pw, data, 0, data.Length);                              

                _pw += _headlengt;
                _headlengt = -1;

                if (_pw >= _length)
                {

                    if (Datas.Length > _SpLengt)
                    {
                        _SpLengt = Datas.Length;
                        SetSplitPostion(_position);
                    }
                    else
                        RestPostion();
                }                           

                return true;
            }
            else
            {
                return false;
            }

        }
       
    }
}

/*
 * 北风之神SOCKET框架(ZYSocket)
 *  Borey Socket Frame(ZYSocket)
 *  by luyikk@126.com QQ:547386448
 *  Updated 2012-07-18 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace ZYSocket.share
{
    public class ZYNetBufferReadStreamV2 : ZYNetBufferReadStream
    {
        public ZYNetBufferReadStreamV2(): this(0, 4096, 4)
        {

        }


        public ZYNetBufferReadStreamV2(int maxSize):this(0,maxSize,4)
        {
          
        }

        public ZYNetBufferReadStreamV2(int maxSize, int headBit)
            : this(0, maxSize, headBit)
        {

        }
        public ZYNetBufferReadStreamV2(int capacity, int maxSize, int headBit)
            : base(capacity, maxSize, headBit)
        {


        }


        protected override void CheckHeadLengt()
        {
            if (_headlengt == -1)
            {
                //int num = (_length - _pw);
                //if (HeadBit > num)
                //{
                //    return;
                //}

            re:

                while (Datas[_pw] != 0xFF)
                {
                    if (_pw < _length-1)
                        _pw++;
                    else
                        return;
                }

                _pw++;

                if (_pw >= _position-1)
                    return;

                byte lengt;
                uint res;
                if (!ReadUInt32(out res, out lengt))
                    return;

               

                if (res > 0)
                {

                    if (res > MaxSize)
                    {
                        this._headlengt = -1;
                        RestPostion();
                        return;
                    }

                    this._headlengt = (int)res;

                }
                else
                {
                    _pw += lengt;

                    goto re;
                }
            }
        }


        public bool ReadUInt32(out uint val, out byte lengt)
        {

            uint loc3 = 0;
            uint loc1 = 0;
            int loc2 = 0;
            int r = _pw;
            lengt = 0;
            val = 0;

            for (int i = 0; i < 32; i++)
            {
                lengt++;

                if (r + i >= Datas.Length)
                    return false;

                loc3 = Datas[r + i];
                if (loc2 < 32)
                {
                    if (loc3 >= 128)
                    {
                        loc1 = loc1 | (loc3 & 127) << loc2;
                    }
                    else
                    {
                        loc1 = loc1 | loc3 << loc2;
                        break;
                    }
                }
                else
                {

                    do
                    {
                        r++;

                    }
                    while (r < Datas.Length && Datas[r] >= 128);
                    break;
                }
                loc2 = loc2 + 7;
            }


            val = loc1;

            return true;



        }



    }
}

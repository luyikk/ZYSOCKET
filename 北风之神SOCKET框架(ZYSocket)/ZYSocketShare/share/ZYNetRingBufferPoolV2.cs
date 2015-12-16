using System;
using System.Collections.Generic;
using System.Text;

namespace ZYSocket.share
{
    public class ZYNetRingBufferPoolV2:ZYNetRingBufferPool
    {
        public ZYNetRingBufferPoolV2():this(4096)
        {
        }

        public ZYNetRingBufferPoolV2(int maxBuffer):base(maxBuffer,4,false)
        {           
        }


        protected override int GetHeadLengt()
        {
            System.Threading.Monitor.Enter(lockobj);

          
            try
            {
               

                while (Data[_current] != 0xFF && _length > 0)
                {
                    if (_current < MAXSIZE - 1)
                    {
                        _current++;
                        _length--;

                        if (_length == 0)
                            break;
                    }
                    else if (_current == MAXSIZE - 1)
                    {
                        _current = 0;
                        _length--;

                        if (_length == 0)
                            break;
                    }
                }


                if (Length < 4)
                {
                    return 0;
                }
                             

                if (_current > MAXSIZE - 1)
                    _current = 0;

                if (_length == 0)
                    return 0;



                uint val;
                byte lengt;

                if (ReadUInt32(out val, out lengt))
                {
                    if (val < 0)
                    {
                        return 0;
                    }

                    return (int)val;
                }
                else
                    return 0;
            }
            finally
            {
                System.Threading.Monitor.Exit(lockobj);
            }

        }


        public bool ReadUInt32(out uint val, out byte lengt)
        {

            uint loc3 = 0;
            uint loc1 = 0;
            int loc2 = 0;
            int r = _current+1;
            lengt = 0;
            val = 0;

            for (int i = 0; i < 32; i++)
            {
                lengt++;

                if (((r + i)%MAXSIZE) >= Data.Length)
                    return false;

                loc3 = Data[((r + i) % MAXSIZE)];
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
                    while (r < _length && Data[(r % MAXSIZE)] >= 128);
                    break;
                }
                loc2 = loc2 + 7;
            }
            val = loc1;
            return true;
        }

        public override bool Read(out byte[] data)
        {
            int count = GetHeadLengt();

            if (count < 0)
            {
                data = null;
                return false;
            }


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

           


            if (count > _length-1)
            {

                data = null;
                return false;
            }

            _current += 1;

            if (Length > 0)
                Length -= 1;

            data = Read(count);

            return true;
        }
    }
}

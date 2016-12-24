using System;
using System.Collections.Generic;
using System.Text;

namespace ZYSocket.ZYNet.Client
{
    public  class ReadBytesV2:ReadBytes
    {

        public ReadBytesV2(byte[] data):base(data)
        {            
            //if (data[0] == 0xff)
            //    base.current = 1;
        }

        public ReadBytesV2(Byte[] data, RDataExtraHandle dataExtraCallBack)
        {
          

            try
            {
                //if (data[0] == 0xff)
                //    base.current = 1;

                Data = data;
                this.startIndex = ReadHeadLengt();

                this.Length = data.Length;
                endlengt = Length - startIndex;               

                byte[] handBytes = new byte[this.startIndex];

                Buffer.BlockCopy(data, 0, handBytes, 0, handBytes.Length); //首先保存不需要解密的数组

                byte[] NeedExByte = new byte[endlengt];

                Buffer.BlockCopy(data, startIndex, NeedExByte, 0, NeedExByte.Length);

                if (dataExtraCallBack != null)
                    NeedExByte = dataExtraCallBack(NeedExByte);

                Data = new byte[handBytes.Length + NeedExByte.Length]; //重新整合解密完毕后的数据包

                Buffer.BlockCopy(handBytes, 0, Data, 0, handBytes.Length);
                Buffer.BlockCopy(NeedExByte, 0, Data, handBytes.Length, NeedExByte.Length);              
                current = 0;
                IsDataExtraSuccess = true;
            }
            catch
            {
                IsDataExtraSuccess = false;
            }
        }

        private int ReadHeadLengt()
        {
            uint loc3 = 0;
            uint loc1 = 0;
            int loc2 = 0;
            int c = current;

            while (true)
            {
                byte tm;

                if (!ReadByte(out tm))
                    break;

               

                loc3 = tm;
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
                    byte sm;
                    do
                    {
                      
                    }
                    while (ReadByte(out sm) && sm >= 128);
                    break;
                }
                loc2 = loc2 + 7;
            }

            int x= current - c;

            current = c;

            return x;
        }


        public override bool ReadInt32(out int values)
        {
            uint loc3 = 0;
            uint loc1 = 0;
            int loc2 = 0;
            for (; ; )
            {
                byte tm;
                if(!ReadByte(out tm))
                    break;
                loc3 = tm;
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
                    byte sm;
                    do
                    {
                       
                    }
                    while (ReadByte(out sm) && sm >= 128);
                    break;
                }
                loc2 = loc2 + 7;
            }

            values= (int)loc1;
            return true;
        }

        public override int ReadInt32()
        {
            uint loc3 = 0;
            uint loc1 = 0;
            int loc2 = 0;
            for (; ; )
            {
                byte tm;
                if (!ReadByte(out tm))
                    break;
                loc3 = tm;
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
                    byte sm;
                    do
                    {

                    }
                    while (ReadByte(out sm) && sm >= 128);
                    break;
                }
                loc2 = loc2 + 7;
            }

            return (int)loc1;                         
        }
    }
}

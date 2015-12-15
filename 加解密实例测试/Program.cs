using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;
using ZYSocket.Security;
using ZYSocket.Compression;
using System.IO;

namespace test1
{
    class Program
    {
        static byte[] DESkeys = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        static byte[] AESkeys = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56,  0x78, 0x90, 0xAB, 0xCD, 0xEF };

        static void Main(string[] args)
        {
            #region DES

            BufferFormat fan = new BufferFormat(1000, new FDataExtraHandle((o) =>
            {
                return DES.EncryptDES(o, DESkeys, "hello word");
            }));

            fan.AddItem(true);
            fan.AddItem("abc");
            fan.AddItem(123);

            byte[] data = fan.Finish();


            ReadBytes read = new ReadBytes(data, 4, -1, new RDataExtraHandle((o) =>
                {
                    return DES.DecryptDES(o, DESkeys, "hello word");
                }));

            int lengt;
            int cmd;
            bool var1;
            string var2;
            int var3;

            if (read.IsDataExtraSuccess &&
                read.ReadInt32(out lengt) &&
                lengt == read.Length &&
                read.ReadInt32(out cmd) &&
                read.ReadBoolean(out var1) &&
                read.ReadString(out var2) &&
                read.ReadInt32(out var3))
            {
                Console.WriteLine("This DES-> Length:{0} Cmd:{1} var1:{2} var2:{3} var3:{4}", lengt, cmd, var1, var2, var3);

            }
            #endregion

            //AES测试
            AEStest();

            //数据压缩
            Deflatetest();



            Console.ReadLine();
        }

        static void Deflatetest()
        {
            BufferFormat fan = new BufferFormat(1000, new FDataExtraHandle((o) =>
            {
                return Deflate.Compress(o);
            }));

            fan.AddItem(true);
            fan.AddItem("abcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabc");
            fan.AddItem(123);

            byte[] data = fan.Finish();


            ReadBytes read = new ReadBytes(data, 4, -1, new RDataExtraHandle((o) =>
            {
                 return Deflate.Decompress(o);
            }));

            int lengt;
            int cmd;
            bool var1;
            string var2;
            int var3;

            if (read.IsDataExtraSuccess &&
                read.ReadInt32(out lengt) &&
                lengt == read.Length &&
                read.ReadInt32(out cmd) &&
                read.ReadBoolean(out var1) &&
                read.ReadString(out var2) &&
                read.ReadInt32(out var3))
            {
                Console.WriteLine("压缩前长度:{0}", read.Data.Length);
                Console.WriteLine("压缩后长度:{0}", read.Length);
                Console.WriteLine("This Deflate-> Length:{0} Cmd:{1} var1:{2} var2:{3} var3:{4}", lengt, cmd, var1, var2, var3);

            }
        }

        static void AEStest()
        {
            BufferFormat fan = new BufferFormat(1000, new FDataExtraHandle((o) =>
            {
                return AES.AESEncrypt(o, AESkeys, "hello wordhello wordhello wordhello wordhello wordhello wordhello wordhello wordhello wordhello wordhello wordhello wordhello wordhello word");
            }));

            fan.AddItem(true);
            fan.AddItem("abc");
            fan.AddItem(123);

            byte[] data = fan.Finish();


            ReadBytes read = new ReadBytes(data, 4, -1, new RDataExtraHandle((o) =>
            {
                return AES.AESDecrypt(o, AESkeys, "hello wordhello wordhello wordhello wordhello wordhello wordhello wordhello wordhello wordhello wordhello wordhello wordhello wordhello word");
            }));

            int lengt;
            int cmd;
            bool var1;
            string var2;
            int var3;

            if (read.IsDataExtraSuccess&&
                read.ReadInt32(out lengt) &&
                lengt == read.Length &&
                read.ReadInt32(out cmd) &&
                read.ReadBoolean(out var1) &&
                read.ReadString(out var2) &&
                read.ReadInt32(out var3))
            {
                Console.WriteLine("This AES-> Length:{0} Cmd:{1} var1:{2} var2:{3} var3:{4}", lengt, cmd, var1, var2, var3);

            }
        }

    }
}

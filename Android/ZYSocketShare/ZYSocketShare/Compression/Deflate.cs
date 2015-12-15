using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace ZYSocket.Compression
{
    /// <summary>
    /// 数据压缩 Deflate 算法
    /// </summary>
    public static class Deflate
    {
        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] bytes)
        {
            try
            {
                byte[] buffer2;
                using (MemoryStream stream = new MemoryStream(bytes, false))
                {
                    using (DeflateStream stream2 = new DeflateStream(stream, CompressionMode.Decompress, false))
                    {
                        using (MemoryStream stream3 = new MemoryStream())
                        {
                            int num;
                            byte[] buffer = new byte[bytes.Length];
                            while ((num = stream2.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                stream3.Write(buffer, 0, num);
                            }
                            stream3.Close();
                            buffer2 = stream3.ToArray();
                        }
                    }
                }
                if (buffer2.Length == 0)
                    return bytes;
                else
                    return buffer2;
            }
            catch
            {
                return bytes;
            }
        }

        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (DeflateStream stream2 = new DeflateStream(stream, CompressionMode.Compress, false))
                {
                    stream2.Write(bytes, 0, bytes.Length);
                }
                stream.Close();
                return stream.ToArray();
            }
        }

 


 

    }
}

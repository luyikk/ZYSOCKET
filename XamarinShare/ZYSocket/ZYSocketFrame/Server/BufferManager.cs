/*
 * 北风之神SOCKET框架(ZYSocket)
 *  Borey Socket Frame(ZYSocket)
 *  by luyikk@126.com
 *  Updated 2010-12-26 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace ZYSocket.Server
{
   
    /// <summary>
    /// 数据包缓冲池
    /// </summary>
    internal sealed class BufferManager
    {
        private Byte[] buffer;
        private Int32 bufferSize;
        private Int32 numSize;
        private Int32 currentIndex;
        private Stack<Int32> freeIndexPool;

        public BufferManager(Int32 numsize, Int32 buffersize)
        {
            this.numSize = numsize;
            this.bufferSize = buffersize;
           
        }

        public void Inint()
        {
            buffer = new byte[numSize];
            freeIndexPool = new Stack<int>(numSize / bufferSize);
        }

        internal void FreeBuffer(SocketAsyncEventArgs args)
        {
            freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
          
        }

        internal Boolean SetBuffer(SocketAsyncEventArgs args)
        {
            if (this.freeIndexPool.Count > 0)
            {
                args.SetBuffer(this.buffer, this.freeIndexPool.Pop(), this.bufferSize);
            }
            else
            {
                if ((this.numSize - this.bufferSize) < this.currentIndex)
                {
                    return false;
                }
                args.SetBuffer(this.buffer, this.currentIndex, this.bufferSize);
                this.currentIndex += this.bufferSize;
            }
            return true;
        }
    }
}

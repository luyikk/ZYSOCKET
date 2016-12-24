using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYSocket.Server
{
    public interface IEnsureSend
    {
        bool EnsureSend(byte[] data);
    }
}

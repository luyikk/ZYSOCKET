using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;
namespace PackHandler
{
    [FormatClassAttibutes((int)PackType.MoveFileSystem)]
    public class MoveFileSystem
    {

        public FileType FileType { get; set; }
        public string OldName { get; set; }
        public string NewName { get; set; }
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }

    }
}

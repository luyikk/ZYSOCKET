using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;

namespace PackHandler
{
    [FormatClassAttibutes((int)PackType.DelFile)]
    public class DelFile
    {
        public List<DelFileName> DelFileList { get; set; }

    }

    public class DelFileName
    {
        public FileType FType { get; set; }
        public string FullName { get; set; }
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }
    }
}

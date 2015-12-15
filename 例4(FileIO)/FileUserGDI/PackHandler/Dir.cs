using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;
namespace PackHandler
{
    [FormatClassAttibutes((int)PackType.Dir)]
    public class Dir
    {
        public string DirName { get; set; }
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }
        public List<FileSystem> FileSystemList { get; set; }
    }

    public enum FileType
    {
        Dir=0x01,
        File=0x02
    }

    public class FileSystem
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public FileType FileType { get; set; }
        public long Size { get; set; }
        public DateTime EditTime { get; set; }
    }
}

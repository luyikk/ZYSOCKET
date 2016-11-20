using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.share;

namespace DrawBoardPACK
{
    [FormatClassAttibutes(1000)]
    public class LogOn
    {
        public string  UserName { get; set; }

        public bool Success { get; set; }
    }

    [FormatClassAttibutes(2000)]
    public class DrawPoint
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Color { get; set; }
        
    }

    [FormatClassAttibutes(3000)]
    public class ClecrColor
    {
        public int Color { get; set; }
    }

    [FormatClassAttibutes(4000)]
    public class SaveImg
    {
        public string FileName { get; set; }

    }

    [FormatClassAttibutes(5000)]
    public class UserCount
    {
        public int Count { get; set; }
    }


}

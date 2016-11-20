using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FileManager
{
    class Program
    {

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Server());
        }

    }
}

using System;
using System.Windows.Forms;

namespace MetroTcpPing
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            ////https://stackoverflow.com/questions/8632375/datagridview-vertical-scrollbar-not-updating-properly-forms-bug
        }
    }
}

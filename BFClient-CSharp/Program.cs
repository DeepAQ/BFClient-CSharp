using System;
using System.Windows.Forms;
using BFClient_CSharp.Util;
using BFClient_CSharp.View;

namespace BFClient_CSharp
{
    internal static class Program
    {
        /// <summary>
        ///     应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (SessionMgr.TryAutoLogin())
                Application.Run(new MainForm());
            else
                Application.Run(new LoginForm());
        }
    }
}
using System;
using System.Net.Sockets;
using System.Windows;


namespace Gold
{
    public partial class App : Application
    {
        public static string clientName;
        public static Socket clientSocket;

        [STAThread]
        static void Main()
        {
            p_log frmLogin = new p_log();
            if (frmLogin.ShowDialog() == true)
            {
                clientName = frmLogin.Client_Name;
                clientSocket = frmLogin.clientSocket;
                program mainProg = new program();
                Application app = new Application();
                app.Run(mainProg);
            }
        }
    }
}

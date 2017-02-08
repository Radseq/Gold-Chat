using System;
using System.Windows;

namespace Gold
{
    public partial class App : Application
    {
        public static string clientName;
        //public static Socket clientSocket;
        // public static ClientManager clientManager;
        static ClientManager cm = new ClientManager();

        /* to use main method like this we need to right click App.xaml and enter to properties select Build Action and check Page
         */

        [STAThread]
        static void Main()
        {
            p_log frmLogin = new p_log(cm);
            if (frmLogin.ShowDialog() == true)
            {
                clientName = frmLogin.userName;
                //clientSocket = frmLogin.clientSocket;
                program mainProg = new program(cm);
                Application app = new Application();
                app.Run(mainProg);
            }
        }
    }
}

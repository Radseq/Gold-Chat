using Gold_Client.Model;
using Gold_Client.View;
using System;
using System.Windows;

namespace Gold_Client
{
    public partial class App : Application
    {
        public static Client Client = new Client();
        /* to use main method like this we need to right click App.xaml and enter to properties select Build Action and check Page*/

        [STAThread]
        static void Main()
        {
            LoginWindow loginWindow = new LoginWindow();
            Application app = new Application();
            app.Run(loginWindow);
            // loginWindow.Show();
            if (loginWindow.ShowDialog() == true)
            {
                MainProgramWindow mainProg = new MainProgramWindow();
                mainProg.ShowDialog();
            }
        }
    }
}

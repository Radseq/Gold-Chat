using Gold_Client.Model;
using System.Windows;

namespace Gold_Client
{
    public partial class App : Application
    {
        public static Client Client = Client.Instance;

        //[STAThread]
        //static void Main()
        //{
        //    MainProgramWindow mainProg = new MainProgramWindow();
        //    Application app = new Application();
        //    app.Run(mainProg);
        //}
    }
}

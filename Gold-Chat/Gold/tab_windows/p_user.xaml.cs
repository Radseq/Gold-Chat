using CommandClient;
using System.Windows.Controls;

namespace Gold.tab_windows
{
    /// <summary>
    /// Interaction logic for p_user.xaml
    /// </summary>
    public partial class p_user : UserControl
    {

        private ClientManager clientManager;

        public p_user(ClientManager cM)
        {
            InitializeComponent();
            clientManager = cM;

            //when tab windows shows up read config and setting value of windows element
            if (clientManager.config.loginEmailNotyfication == false)
                emailNotyfiCb.IsChecked = false;
            else emailNotyfiCb.IsChecked = true;
        }

        private void button1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (newPassTb.Password != "" && confNewPassTb.Password != "" && newPassTb.Password.Length > 6)
            {
                Data msgToSend = new Data();

                msgToSend.strName = App.clientName;
                msgToSend.strMessage = clientManager.CalculateChecksum(confNewPassTb.Password);
                //msgToSend.strMessage2 = emailTextbox.Text;
                msgToSend.cmdCommand = Command.changePassword;

                byte[] byteData = msgToSend.ToByte();

                clientManager.BeginSend(byteData);
            }
        }

        private void emailNotyfiCb_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            clientManager.config.loginEmailNotyfication = false; //user dont want to be notyficated
        }

        private void emailNotyfiCb_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            clientManager.config.loginEmailNotyfication = true; //user want to be notyficated
        }
    }
}

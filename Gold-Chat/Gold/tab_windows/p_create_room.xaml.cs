using CommandClient;
using System.Windows.Controls;

namespace Gold.tab_windows
{
    /// <summary>
    /// Interaction logic for p_create_room.xaml
    /// </summary>
    public partial class p_create_room : UserControl
    {
        ClientManager clientManager;

        public p_create_room(ClientManager cm)
        {
            InitializeComponent();
            clientManager = cm;
        }

        private void create_channel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Data msgToSend = new Data();

            msgToSend.strName = clientManager.userName; //channel admin
            msgToSend.strMessage = roomNameTb.Text;
            msgToSend.strMessage2 = clientManager.CalculateChecksum(enterPass.Password);
            msgToSend.strMessage3 = clientManager.CalculateChecksum(amdinPass.Password);
            msgToSend.strMessage4 = welcomeMessageTb.Text;
            msgToSend.cmdCommand = Command.createChannel;

            byte[] byteData = msgToSend.ToByte();
            clientManager.BeginSend(byteData);
        }
    }
}

using CommandClient;
using System.Windows;

namespace Gold
{
    /// <summary>
    /// Interaction logic for serverAsk.xaml
    /// </summary>
    public partial class serverAsk : Window
    {
        private ClientManager clientManager;
        private string message1;
        private string message2;

        public serverAsk(ClientManager cm, string msg, string msg2)
        {
            InitializeComponent();
            clientManager = cm;
            message1 = msg;
            message2 = msg2;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Data msgToSend = new Data();

            msgToSend.strName = App.clientName; //channel admin
            msgToSend.strMessage = message1;
            msgToSend.strMessage2 = clientManager.CalculateChecksum(answerTB.Text);
            msgToSend.cmdCommand = Command.joinChannel;

            byte[] byteData = msgToSend.ToByte();
            clientManager.BeginSend(byteData);
        }
    }
}

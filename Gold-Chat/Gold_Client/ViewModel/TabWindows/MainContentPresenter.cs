using CommandClient;
using Gold_Client.Model;
using Gold_Client.ViewModel.Others;
using System.Windows.Input;

namespace Gold_Client.ViewModel.TabWindows
{
    public class MainContentPresenter : ObservableObject
    {
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        ProcessReceivedByte proccesReceiverInformation = new ProcessReceivedByte();

        private string incomeMsg;
        private string outcomeMsg;

        public MainContentPresenter()
        {
            proccesReceiverInformation.ProccesBuffer();
            proccesReceiverInformation.ClientLogin += OnClientLogin;
            proccesReceiverInformation.ClientLogout += OnClientLogout;
            proccesReceiverInformation.ClientMessage += OnClientMessage;
        }

        public string IncomeMessageTB
        {
            get { return incomeMsg; }
            set
            {
                incomeMsg = value;
                RaisePropertyChangedEvent(nameof(IncomeMessageTB));
            }
        }

        public string OutcomeMessageTB
        {
            get { return outcomeMsg; }
            set
            {
                outcomeMsg = value;
                RaisePropertyChangedEvent(nameof(OutcomeMessageTB));
            }
        }

        public ICommand MessageCommand => new DelegateCommand(() =>
        {
            if (string.IsNullOrWhiteSpace(OutcomeMessageTB)) return;
            clientSendToServer.SendToServer(Command.Message, OutcomeMessageTB);
            OutcomeMessageTB = string.Empty;
        });

        private void OnClientLogin(object sender, ClientEventArgs e)
        {
            //if (!usersConnected.Contains(e.clientLoginName))
            //{
            //    usersConnected.Add(e.clientLoginName);
            IncomeMessageTB += e.clientLoginMessage + "\r\n";
            // }
        }

        private void OnClientLogout(object sender, ClientEventArgs e)
        {
            // usersConnected.Remove(e.clientLogoutMessage);
            IncomeMessageTB += "<<<" + e.clientLogoutMessage + " has left the room>>>" + "\r\n";
        }

        private void OnClientMessage(object sender, ClientEventArgs e)
        {
            IncomeMessageTB += e.clientMessage;
        }
    }
}

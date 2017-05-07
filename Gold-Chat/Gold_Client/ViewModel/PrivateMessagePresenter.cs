using CommandClient;
using Gold_Client.Model;
using System.Windows.Input;

namespace Gold_Client.ViewModel
{
    class PrivateMessagePresenter : ObservableObject
    {

        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        ClientReceivedFromServer clientReceiveFromServer = ClientReceivedFromServer.Instance;

        private string incomePrivMessage;
        private string outGoingPrivMessage;
        public string FriendName;

        public PrivateMessagePresenter()
        {
            clientReceiveFromServer.ClientPrivMessage += OnClientPrivMessage;
        }

        public string IncomePrivMessage
        {
            get { return incomePrivMessage; }
            set
            {
                incomePrivMessage = value;
                RaisePropertyChangedEvent(nameof(IncomePrivMessage));
            }
        }

        public string OutGoingPrivMessage
        {
            get { return outGoingPrivMessage; }
            set
            {
                outGoingPrivMessage = value;
                RaisePropertyChangedEvent(nameof(OutGoingPrivMessage));
            }
        }

        public ICommand SendPrivateMessageCommand => new DelegateCommand(() =>
        {
            if (string.IsNullOrWhiteSpace(OutGoingPrivMessage)) return;
            clientSendToServer.SendToServer(Command.privMessage, OutGoingPrivMessage, friendName);
        });

        private void OnClientPrivMessage(object sender, ClientEventArgs e)
        {
            friendName = e.clientFriendName;
            IncomePrivMessage += e.clientPrivMessage;
        }
    }
}

using CommandClient;
using Gold_Client.Model;
using Gold_Client.ViewModel.Others;
using System.Windows.Input;

namespace Gold_Client.ViewModel
{
    class PrivateMessagePresenter : ObservableObject
    {
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        ProcessReceivedByte getMessageFromServer = ProcessReceivedByte.Instance;

        private string incomePrivMessage;
        private string outGoingPrivMessage;

        private string privateWindowTitle;
        string friendName;

        public string FriendName
        {
            get
            {
                return friendName;
            }
            set
            {
                PrivateWindowTitle = "Private Message with " + value;
                friendName = value;
            }
        }

        public PrivateMessagePresenter()
        {
            getMessageFromServer.ClientPrivMessage += OnClientPrivMessage;
            getMessageFromServer.ClientLogout += OnClientLogout;
        }

        private void OnClientLogout(object sender, ClientEventArgs e)
        {
            IncomePrivMessage += "<<<" + e.clientLogoutMessage + " has logout >>>" + "\r\n";
        }

        public void ShowFirstMessageWhenWindowShow(string message)
        {
            IncomePrivMessage = message + "\r\n";
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

        public string PrivateWindowTitle
        {
            get { return privateWindowTitle; }
            set
            {
                privateWindowTitle = value;
                RaisePropertyChangedEvent(nameof(PrivateWindowTitle));
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
            clientSendToServer.SendToServer(Command.privMessage, OutGoingPrivMessage, FriendName);
            OutGoingPrivMessage = null;
        });

        private void OnClientPrivMessage(object sender, ClientEventArgs e)
        {
            FriendName = e.clientFriendName;
            IncomePrivMessage += e.clientPrivMessage + "\r\n";
        }
    }
}

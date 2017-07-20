using CommandClient;
using Gold_Client.Model;
using Gold_Client.ViewModel.Others;
using System;
using System.Windows.Input;

namespace Gold_Client.ViewModel.TabWindows
{
    public class GlobalMessageContentPresenter : ObservableObject
    {
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        ProcessReceivedByte getMessageFromServer = ProcessReceivedByte.Instance;

        private string incomeMsg;
        private string outcomeMsg;

        public GlobalMessageContentPresenter()
        {
            getMessageFromServer.ClientLogin += OnClientLogin;
            getMessageFromServer.ClientLogout += OnClientLogout;
            getMessageFromServer.ClientMessage += OnClientMessage;
            getMessageFromServer.ClientKickFromServer += OnClientKickFromServer;
            getMessageFromServer.ClientBanFromServer += OnClientBanFromServer;
        }

        private void OnClientBanFromServer(object sender, ClientEventArgs e)
        {
            if (e.clientName != App.Client.strName)
                IncomeMessageTB = e.clientBanReason + "\r\n";
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
            IncomeMessageTB += e.clientLoginMessage + "\r\n";
        }

        private void OnClientLogout(object sender, ClientEventArgs e)
        {
            IncomeMessageTB += "<<<" + e.clientLogoutMessage + " has left the room>>>" + "\r\n";
        }

        private void OnClientMessage(object sender, ClientEventArgs e)
        {
            IncomeMessageTB += e.clientMessage + Environment.NewLine; // Or + "\r\n" 
        }

        private void OnClientKickFromServer(object sender, ClientEventArgs e)
        {
            if (e.clientName != App.Client.strName)
            {
                IncomeMessageTB += e.clientName + " has kicked, " + e.clientKickReason + "\r\n";
            }
        }
    }
}

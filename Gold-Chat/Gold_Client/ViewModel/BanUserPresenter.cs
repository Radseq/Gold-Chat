using CommandClient;
using Gold_Client.ViewModel.Others;
using System.Windows;
using System.Windows.Input;

namespace Gold_Client.ViewModel
{
    public class BanUserPresenter : ObservableObject
    {
        public string UserName;
        public string ChannelName;

        string banReason;
        string endBanTime;

        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;

        public string BanReasonTextBox
        {
            get { return banReason; }
            set
            {
                banReason = value;
                RaisePropertyChangedEvent(nameof(BanReasonTextBox));
            }
        }

        public string DateTimeValue
        {
            get { return endBanTime; }
            set
            {
                endBanTime = value;
                RaisePropertyChangedEvent(nameof(DateTimeValue));
            }
        }

        public ICommand BanCommand => new DelegateCommand(() =>
        {
            if (!string.IsNullOrWhiteSpace(BanReasonTextBox) && !string.IsNullOrWhiteSpace(DateTimeValue) && !string.IsNullOrWhiteSpace(ChannelName))
            { // Channel ban
                clientSendToServer.SendToServer(Command.banUserChannel, UserName, endBanTime, banReason, ChannelName);
            }
            else if (!string.IsNullOrWhiteSpace(BanReasonTextBox) && !string.IsNullOrWhiteSpace(DateTimeValue) && string.IsNullOrWhiteSpace(ChannelName))
            { // Server Ban
                clientSendToServer.SendToServer(Command.ban, UserName, endBanTime, banReason);
            }
            else
                MessageBox.Show("Ban Time or reason cant be null", "Login Information", MessageBoxButton.OK, MessageBoxImage.Error);

        });
    }
}

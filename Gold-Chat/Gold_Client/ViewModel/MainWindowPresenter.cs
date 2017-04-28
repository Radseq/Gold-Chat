using CommandClient;
using System.Windows.Input;

namespace Gold_Client.ViewModel
{
    class MainWindowPresenter : ObservableObject
    {
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;

        private string incomeMsg;
        public string IncomeMessageTB
        {
            get { return incomeMsg; }
            set
            {
                incomeMsg = value;
                RaisePropertyChangedEvent(nameof(IncomeMessageTB));
            }
        }

        private string outcomeMsg;
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

        public ICommand ControlPanelTabCommand => new DelegateCommand(() =>
        {

        });

        public ICommand CreateRoomTabCommand => new DelegateCommand(() =>
        {

        });

        public ICommand SugestionTabCommand => new DelegateCommand(() =>
        {

        });

        public ICommand AbouseTabCommand => new DelegateCommand(() =>
        {

        });

        public ICommand ArchiveTabCommand => new DelegateCommand(() =>
        {
        });

        public ICommand ContactTabCommand => new DelegateCommand(() =>
        {

        });

        public ICommand AdminTabCommand => new DelegateCommand(() =>
        {

        });

        public ICommand InformationTabCommand => new DelegateCommand(() =>
        {

        });

        public ICommand LogoutCommand => new DelegateCommand(() =>
        {

        });

        public ICommand AddFriendHandleCommand => new DelegateCommand(() =>
        {

        });
    }
}

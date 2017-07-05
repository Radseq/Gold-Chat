using System.Windows;

namespace Gold_Client.View
{
    /// <summary>
    /// Interaction logic for SendFileWindow.xaml
    /// </summary>
    public partial class SendFileWindow : Window
    {
        public SendFileWindow(string UserNameToSendFile)
        {
            InitializeComponent();
            if (DataContext != null)
            {
                ((dynamic)DataContext).NameOfUserToSendFile = UserNameToSendFile;
            }
        }
    }
}

using System.Windows;

namespace Gold_Client.View
{
    public partial class PrivateMessageWindow : Window
    {
        public PrivateMessageWindow(string FriendName)
        {
            InitializeComponent();
            //TODO Debug and see because should be in Control not window
            if (DataContext != null)
            { ((dynamic)DataContext).FriendName = FriendName; }
        }
    }
}

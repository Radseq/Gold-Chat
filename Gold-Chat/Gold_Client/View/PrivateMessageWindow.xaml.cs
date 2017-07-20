using System.Windows;

namespace Gold_Client.View
{
    public partial class PrivateMessageWindow : Window
    {
        public PrivateMessageWindow(string FriendName, string firstMessage = null)
        {
            InitializeComponent();
            if (DataContext != null)
            {
                ((dynamic)DataContext).FriendName = FriendName;
                if (firstMessage != null)
                    ((dynamic)DataContext).ShowFirstMessageWhenWindowShow(firstMessage);
            }
        }
    }
}

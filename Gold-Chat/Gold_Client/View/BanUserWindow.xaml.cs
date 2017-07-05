using System.Windows;

namespace Gold_Client.View
{
    /// <summary>
    /// Interaction logic for BanUserWindow.xaml
    /// </summary>
    public partial class BanUserWindow : Window
    {
        public BanUserWindow(string userName, string channelName)
        {
            InitializeComponent();
            if (DataContext != null)
            {
                ((dynamic)DataContext).UserName = userName;
                if (channelName != null)
                    ((dynamic)DataContext).ChannelName = channelName;
            }
        }
    }
}

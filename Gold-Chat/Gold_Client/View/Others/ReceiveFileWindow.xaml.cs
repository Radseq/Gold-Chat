using System.Windows;

namespace Gold_Client.View.Others
{
    /// <summary>
    /// Interaction logic for ReceiveFileWindow.xaml
    /// </summary>
    public partial class ReceiveFileWindow : Window
    {
        public ReceiveFileWindow(string FriendName, string FileName, long FileLen)
        {
            InitializeComponent();
            if (DataContext != null)
                ((dynamic)DataContext).SetProperies(FriendName, FileName, FileLen);
        }
    }
}

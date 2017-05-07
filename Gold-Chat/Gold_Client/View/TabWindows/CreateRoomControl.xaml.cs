using System.Windows;
using System.Windows.Controls;

namespace Gold_Client.View.TabWindows
{
    public partial class CreateRoomControl
    {
        public CreateRoomControl()
        {
            InitializeComponent();
        }

        private void enterPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            { ((dynamic)DataContext).EnterSecurePassword = ((PasswordBox)sender).SecurePassword; }
        }

        private void confEnterPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            { ((dynamic)DataContext).EnterSecurePasswordRepeart = ((PasswordBox)sender).SecurePassword; }
        }
        private void adminPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            { ((dynamic)DataContext).AdminSecurePassword = ((PasswordBox)sender).SecurePassword; }
        }

        private void confAdminPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            { ((dynamic)DataContext).AdminSecurePasswordRepeart = ((PasswordBox)sender).SecurePassword; }
        }
    }
}

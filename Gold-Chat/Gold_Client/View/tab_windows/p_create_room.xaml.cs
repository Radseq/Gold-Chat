using System.Windows;
using System.Windows.Controls;

namespace Gold_Client.View.tab_windows
{
    public partial class p_create_room
    {
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

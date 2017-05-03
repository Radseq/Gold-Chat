using System.Windows;
using System.Windows.Controls;

namespace Gold_Client.View
{
    public partial class LostPasswordControl
    {
        public LostPasswordControl()
        {
            InitializeComponent();
        }

        private void newPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            { ((dynamic)DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword; }
        }

        private void newPassword2Changed(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            { ((dynamic)DataContext).SecurePasswordRepeart = ((PasswordBox)sender).SecurePassword; }
        }
    }
}

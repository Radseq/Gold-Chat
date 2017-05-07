using System.Windows;
using System.Windows.Controls;

namespace Gold_Client.View.TabWindows
{
    public partial class SettingsControl
    {
        public SettingsControl()
        {
            InitializeComponent();
        }

        private void NewPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            { ((dynamic)DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword; }
        }

        private void ConfNewPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            { ((dynamic)DataContext).SecurePasswordRepeart = ((PasswordBox)sender).SecurePassword; }
        }
    }
}

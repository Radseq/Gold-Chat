using System.Windows;
using System.Windows.Controls;

namespace Gold_Client.View
{
    public partial class LoginControl
    {
        public LoginControl()
        {
            InitializeComponent();
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            { ((dynamic)DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword; }
        }
    }
}

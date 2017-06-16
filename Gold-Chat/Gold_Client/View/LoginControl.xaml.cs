using Gold_Client.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Gold_Client.View
{
    public partial class LoginControl
    {
        public LoginControl()
        {
            LoginPresenter presenter = new LoginPresenter();
            DataContext = presenter;
            InitializeComponent();
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            { ((dynamic)DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword; }
        }
    }
}

using System.Windows.Controls;
using System.Windows.Input;

namespace Gold_Client.ViewModel
{
    public class LoginPresenter : ObservableObject
    {

        private string login;
        public string LoginTB
        {
            get { return login; }
            set
            {
                login = value;
                RaisePropertyChangedEvent(nameof(LoginTB));
            }
        }

        private void Login(object obj)
        {
            PasswordBox pwBox = obj as PasswordBox;
            if (string.IsNullOrWhiteSpace(login) && string.IsNullOrWhiteSpace(pwBox.Password)) return;
            ClientLogin clientLogin = new ClientLogin(App.Client);
            clientLogin.SendLoginAndEncryptPass(login, pwBox);
        }

        public ICommand RegistrationWindowCommand => new DelegateCommand(() =>
        {
            //p_reg reg = new p_reg(clientManager, this);
            //reg.ShowDialog();
        });

        public ICommand LostPasswordWindowCommand => new DelegateCommand(() =>
        {
            //lost_password rp = new lost_password(clientManager);
            //rp.Show();
        });
    }
}

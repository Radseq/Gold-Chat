using CommandClient;
using Gold_Client.Model;
using Gold_Client.ViewModel.Others;
using System.Security;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Gold_Client.ViewModel
{
    public class RegistrationPresenter : ObservableObject
    {
        private string registrationEmailTB;
        private string registrationLoginTB;

        private bool registrationLoginTBIsFocusable;
        private bool passwordIsFocusable;
        private bool emailTBIsFocusable;

        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        ProcessReceivedByte getMessageFromServer = ProcessReceivedByte.Instance;

        public SecureString SecurePassword { private get; set; }
        public SecureString SecurePasswordRepeart { private get; set; }

        public RegistrationPresenter()
        {
            getMessageFromServer.ClientRegistration += OnClientReceiveFromServer;
        }

        private void OnClientReceiveFromServer(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientRegMessage, "Registration Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool RegistrationLoginTBIsFocusable
        {
            get { return registrationLoginTBIsFocusable; }
            set
            {
                registrationLoginTBIsFocusable = value;
                RaisePropertyChangedEvent(nameof(RegistrationLoginTBIsFocusable));
            }
        }

        public bool PasswordIsFocusable
        {
            get { return passwordIsFocusable; }
            set
            {
                passwordIsFocusable = value;
                RaisePropertyChangedEvent(nameof(PasswordIsFocusable));
            }
        }

        public bool EmailTBIsFocusable
        {
            get { return emailTBIsFocusable; }
            set
            {
                emailTBIsFocusable = value;
                RaisePropertyChangedEvent(nameof(EmailTBIsFocusable));
            }
        }

        public string RegistrationLoginTB
        {
            get { return registrationLoginTB; }
            set
            {
                registrationLoginTB = value;
                RaisePropertyChangedEvent(nameof(RegistrationLoginTB));
            }
        }

        public string RegistrationEmailTB
        {
            get { return registrationEmailTB; }
            set
            {
                registrationEmailTB = value;
                RaisePropertyChangedEvent(nameof(RegistrationEmailTB));
            }
        }

        public ICommand RegistrationCommand => new DelegateCommand(() =>
        {
            ProccesAndExecuteInputs(RegistrationLoginTB, RegistrationEmailTB,
                (new System.Net.NetworkCredential(string.Empty, SecurePassword).Password),
                (new System.Net.NetworkCredential(string.Empty, SecurePasswordRepeart).Password));
        });

        public ICommand CleanRegistrationWindowCommand => new DelegateCommand(() =>
        {
            RegistrationEmailTB = "";
            RegistrationLoginTB = "";
        });

        private void ProccesAndExecuteInputs(string login, string email, string/*Hack*/ password, string password2)
        {
            if (login != string.Empty && password != string.Empty && password != string.Empty && email != string.Empty)
            {
                Regex rgx = new Regex(@"^(?=[a-z])[-\w.]{0,23}([a-zA-Z\d])$");
                bool loginRegex = rgx.IsMatch(login);

                if (login.Length < 5 && login.Length > 23)
                {
                    RegistrationLoginTBIsFocusable = true;
                    MessageBox.Show("Your username must be between 5 and 23 chars", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                else if (!loginRegex)
                {
                    RegistrationLoginTBIsFocusable = true;
                    MessageBox.Show("Your username must contain only a-z or 0-9 extample michael123", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                else if (password.Length < 6 && password2.Length < 6)
                {
                    PasswordIsFocusable = true;
                    MessageBox.Show("Your password must be highter than 6 chars", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (SecurePassword != SecurePasswordRepeart)
                {
                    PasswordIsFocusable = true;
                    MessageBox.Show("Password and Repeat Password are not the same", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (email.Length == 0)
                {
                    EmailTBIsFocusable = true;
                    MessageBox.Show("Enter an email.", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                else if (!Regex.IsMatch(email, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
                {
                    EmailTBIsFocusable = true;
                    MessageBox.Show("Enter a valid email.", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    clientSendToServer.SendToServer(Command.Registration, clientSendToServer.CalculateChecksum
                        (new System.Net.NetworkCredential(string.Empty, SecurePassword).Password), email);

                    getMessageFromServer.ClientRegistration += OnClientRegistration;
                }
            }
            else
                MessageBox.Show("Fill in the fields", "Error validation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnClientRegistration(object sender, ClientEventArgs e)
        {
            if (e.clientRegMessage == "You has been registered")
            {
                //loginPanel.loginTextBox.Text = logTextbox.Text;
                //Close();
            }
            else MessageBox.Show(e.clientRegMessage, "Registration Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

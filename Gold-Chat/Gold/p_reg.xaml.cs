using CommandClient;
using System;
//for hash
using System.Windows;

namespace Gold
{
    /// <summary>
    /// Interaction logic for p_reg.xaml
    /// </summary>
    public partial class p_reg : Window
    {
        ClientManager clientManager;
        p_log loginPanel;

        public p_reg(ClientManager cm, p_log pLog)
        {
            loginPanel = pLog;
            clientManager = cm;
            InitializeComponent();
        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            if (logTextbox.Text != string.Empty && passbox.Password != string.Empty && rePassbox.Password != string.Empty && emailTextbox.Text != string.Empty)
            {
                if (logTextbox.Text.Length < 3 && logTextbox.Text.Length > 30 /* todo && !System.Text.RegularExpressions.Regex.IsMatch(logTextbox.Text, @"[a-z]{30}")*/)
                {

                    MessageBox.Show("Your username must be between 4 and 29 chars", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    logTextbox.Focus();
                }

                //if (!System.Text.RegularExpressions.Regex.IsMatch(logTextbox.Text, "^[a-zA-Z0-9_.]+$"))
                //{ 

                //}

                else if (passbox.Password.Length < 6 && rePassbox.Password.Length < 6)
                {

                    MessageBox.Show("Your password must be highter than 6 chars", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    passbox.Focus();
                }
                else if (passbox.Password != rePassbox.Password)
                {

                    MessageBox.Show("Password and Repeat Password are not the same", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    passbox.Focus();
                }
                else if (emailTextbox.Text.Length == 0)
                {
                    MessageBox.Show("Enter an email.", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    emailTextbox.Focus();

                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(emailTextbox.Text, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
                {
                    MessageBox.Show("Enter a valid email.", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    emailTextbox.Select(0, emailTextbox.Text.Length);
                    emailTextbox.Focus();

                }
                else
                {
                    App.clientName = logTextbox.Text;
                    clientManager.SendToServer(Command.Reg, clientManager.CalculateChecksum(passbox.Password), emailTextbox.Text);

                    clientManager.ClientRegistration += OnClientRegistration;
                }
            }
            else
                MessageBox.Show("Fill in the fields", "Error validation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnClientRegistration(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientRegMessage, "Registration Information", MessageBoxButton.OK, MessageBoxImage.Information);
            if (e.clientRegMessage == "You has been registered")
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    loginPanel.loginTextBox.Text = logTextbox.Text;
                    Close();
                }));
            }
        }

        private void cleanButton_Click(object sender, RoutedEventArgs e)
        {
            logTextbox.Text = "";
            passbox.Password = "";
            rePassbox.Password = "";
            emailTextbox.Text = "";
        }
    }
}

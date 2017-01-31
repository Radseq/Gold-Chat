using CommandClient;
//for hash
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace Gold
{
    /// <summary>
    /// Interaction logic for p_reg.xaml
    /// </summary>
    public partial class p_reg : Window
    {
        int error;

        ClientManager clientManager;

        public p_reg(ClientManager cm)
        {
            clientManager = cm;
            InitializeComponent();
        }

        //md5
        private static string CalculateChecksum(string inputString)
        {
            var md5 = new MD5CryptoServiceProvider();
            var hashbytes = md5.ComputeHash(Encoding.UTF8.GetBytes(inputString));
            var hashstring = "";
            foreach (var hashbyte in hashbytes)
                hashstring += hashbyte.ToString("x2");

            return hashstring;
        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            error = 0;

            if (logTextbox.Text != string.Empty && passbox.Password != string.Empty && rePassbox.Password != string.Empty && emailTextbox.Text != string.Empty)
            {
                if (logTextbox.Text.Length < 3 && logTextbox.Text.Length > 30)
                {
                    error++;
                    MessageBox.Show("Your username must be between 4 and 29 chars", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    logTextbox.Focus();
                }

                //if (!System.Text.RegularExpressions.Regex.IsMatch(logTextbox.Text, "^[a-zA-Z0-9_.]+$"))
                //{ 

                //}

                if (passbox.Password.Length < 6 && rePassbox.Password.Length < 6)
                {
                    error++;
                    MessageBox.Show("Your password must be highter than 6 chars", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    passbox.Focus();
                }

                if (passbox.Password != rePassbox.Password)
                {
                    error++;
                    MessageBox.Show("Password and Repeat Password are not the same", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    passbox.Focus();
                }

                if (emailTextbox.Text.Length == 0)
                {
                    MessageBox.Show("Enter an email.", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    emailTextbox.Focus();
                    error++;
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(emailTextbox.Text, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
                {
                    MessageBox.Show("Enter a valid email.", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    emailTextbox.Select(0, emailTextbox.Text.Length);
                    emailTextbox.Focus();
                    error++;
                }
                //else
                //NonQury zwraca ilosc wierszy zmienionych. przy insert 1 elementu jesli jest 1 = insert zrobiony, inaczej to jest blad

            }
            else
                MessageBox.Show("Fill in the fields", "Error validation", MessageBoxButton.OK, MessageBoxImage.Information);


            if (error == 0)
            {
                Data msgToSend = new Data();

                msgToSend.strName = logTextbox.Text;
                msgToSend.strMessage = CalculateChecksum(passbox.Password);
                msgToSend.strMessage2 = emailTextbox.Text;
                msgToSend.cmdCommand = Command.Reg;

                byte[] byteData = msgToSend.ToByte();

                clientManager.BeginConnect();

                clientManager.BeginSend(byteData);

                clientManager.ClientRegistration += OnClientRegistration;
            }
        }

        private void OnClientRegistration(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientRegMessage, "Registration Information", MessageBoxButton.OK, MessageBoxImage.Information);
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

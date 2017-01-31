using MySql.Data.MySqlClient;// for mysql
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

        private MySqlConnection mySqlConn;

        private string server;
        private string database;
        private string uid;
        private string password;
        //private int port;

        public p_reg()
        {
            InitializeComponent();
            server = Settings.DB_HOST;
            database = Settings.DB;
            uid = Settings.DB_ROOT;
            password = Settings.DB_PASS;
            //port = Settings.DB_PORT;
            string connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            //cn = new MySqlConnection("server=db4free.net;uid=a9256518;pwd=atlandb;database=a587644;port=3306;");
            mySqlConn = new MySqlConnection(connectionString);
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
                MySqlCommand mySqlComm = new MySqlCommand("", mySqlConn);
                mySqlConn.Open();
                mySqlComm.CommandText = "SELECT login, email, register_id FROM users WHERE login = @login and email = @email";
                mySqlComm.Parameters.AddWithValue("@login", logTextbox.Text);
                mySqlComm.Parameters.AddWithValue("@email", emailTextbox.Text);

                MySqlDataReader mySqlReader = null;
                mySqlReader = mySqlComm.ExecuteReader();

                string loginExsist = "";
                string emailExsist = "";
                string registerCode = "";

                while (mySqlReader.Read()) // If you're expecting only one line, change this to if(reader.Read()).
                {
                    loginExsist = mySqlReader.GetString(0);
                    emailExsist = mySqlReader.GetString(1);
                    registerCode = mySqlReader.GetString(2);
                }
                mySqlConn.Close();


                if (loginExsist == logTextbox.Text)
                {
                    MessageBox.Show("Your login exists, try other one", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    logTextbox.Focus();
                }
                else if (emailExsist == emailTextbox.Text)
                {
                    MessageBox.Show("Your email exists, try other one", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    emailTextbox.Focus();
                }
                else if (registerCode != "")
                {
                    MessageBox.Show("You have already register, go to login windows and paste register key", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {

                    mySqlComm.CommandText = "INSERT INTO users (login, password, email, register_id) " + "VALUES (@user_name, @user_password, @user_email, @register_id)";

                    string calcPassword = CalculateChecksum(passbox.Password);
                    string registrationCode = CalculateChecksum(emailTextbox.Text);

                    mySqlComm.Parameters.AddWithValue("@user_name", logTextbox.Text);
                    mySqlComm.Parameters.AddWithValue("@user_password", calcPassword);
                    mySqlComm.Parameters.AddWithValue("@user_email", emailTextbox.Text);
                    mySqlComm.Parameters.AddWithValue("@register_id", registrationCode);

                    mySqlConn.Open();
                    if (mySqlComm.ExecuteNonQuery() > 0)
                    {
                        var emailSender = new EmailSender();
                        emailSender.EmailSended += OnEmaiSended;

                        string emailMessage2 = @"< p >
< strong >
< span style = 'color: #ff0000;' > Pamiętaj by dokłanie skopiować KOD.</ span >
</ strong >
</ p >
< p >Dziękujemy < br /> administracja Gold Chat.</ p >";

                        string emailMessage = string.Format(@"
<p>Witaj <strong>{0}</strong>.
    <br />Dziękujemy za rejestrację w aplikacji <strong>Gold Chat</strong>.
    <br />Zanim będziesz mógł kożystać z aplikacji, musisz wykonać ostatnia operacje.
    <br />Pamiętaj - musisz to zrobić zanim staniesz sie w pełni zarejestrowanym użytkownikiem.<br />
    <span style='text-decoration: underline;'>
        <em>Jedyne co musisz zrobić to skopiować kod aktywacyjny, oraz wkleić go w oknie <strong>Register Code</strong> okno to pojawi się gdy wpiszesz swój login i hasło w <strong>Oknie Logowania!.</strong>
        </em></span></p><p><br />A o to twój kod aktywacyjny : <span style='color: #ff0000;' ><strong>{1}</strong></span></ p >{2}", logTextbox.Text, registrationCode, emailMessage2);


                        emailSender.SendEmail(emailTextbox.Text, "Gold Chat: Registration", emailMessage);

                        MessageBox.Show("You has been registered", "Congratulation", MessageBoxButton.OK, MessageBoxImage.Information);

                        mySqlConn.Close();

                        Close();
                    }
                    else
                        MessageBox.Show("Account NOT created with unknown reason.", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        //catch (Exception ex)
        //{
        //MessageBox.Show("Cannot Register.  Contact administrator", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
        //}
        //}

        private void cleanButton_Click(object sender, RoutedEventArgs e)
        {
            logTextbox.Text = "";
            passbox.Password = "";
            rePassbox.Password = "";
            emailTextbox.Text = "";
        }

        private void OnEmaiSended(object source, EmailSenderEventArgs args)
        {
            MessageBox.Show("Activation Code has been send to your email", "Close", MessageBoxButton.OK);
        }
    }
}

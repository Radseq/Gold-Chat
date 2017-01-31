using MySql.Data.MySqlClient;
using System.Windows;

namespace Gold
{
    /// <summary>
    /// Interaction logic for register_code.xaml
    /// </summary>
    public partial class register_code : Window
    {
        MySqlConnection mySqlConn;

        private string regCode;
        public string userName;
        private string userEmail;

        private string server;
        private string database;
        private string uid;
        private string password;

        public register_code()
        {
            InitializeComponent();
            server = Settings.DB_HOST;
            database = Settings.DB;
            uid = Settings.DB_ROOT;
            password = Settings.DB_PASS;
            string connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            mySqlConn = new MySqlConnection(connectionString);
        }

        private void endReg(object sender, RoutedEventArgs e)
        {
            string userRegisterCode = registerCode.Text;

            if (userRegisterCode == "")
            {
                MessageBox.Show("Write your Activation code to text box", "Error, Empty Activation Code", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            mySqlConn.Close();
            mySqlConn.Open();
            string selectQuery = "SELECT register_id, email FROM users WHERE register_id = @register_id AND login = @login ;";
            MySqlCommand mySqlComm = new MySqlCommand(selectQuery, mySqlConn);
            mySqlComm.Parameters.AddWithValue("@register_id", userRegisterCode);
            mySqlComm.Parameters.AddWithValue("@login", userName);

            MySqlDataReader mySqlReader = null;
            mySqlReader = mySqlComm.ExecuteReader();

            //string registerCode = "";

            if (mySqlReader.Read())
            {
                regCode = mySqlReader.GetString(0);
                userEmail = mySqlReader.GetString(1);
            }
            mySqlReader.Close();

            if (regCode == userRegisterCode)
            {
                string updateQuery = "UPDATE users SET register_id = @reg_id WHERE email = @email ;";
                MySqlCommand mySqlCommUpdate = new MySqlCommand(updateQuery, mySqlConn);

                mySqlCommUpdate.Parameters.AddWithValue("@reg_id", "");
                mySqlCommUpdate.Parameters.AddWithValue("@email", userEmail);

                if (mySqlCommUpdate.ExecuteNonQuery() > 0)
                {
                    MessageBox.Show("Now you can login in to application", "Close", MessageBoxButton.OK);
                }
                else
                {
                    MessageBox.Show("Unknow Error.", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                mySqlConn.Close();
                Close();
            }
            else
            {
                MessageBox.Show("Activation code not match.", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            mySqlConn.Close();
        }

        private void resendCode(object sender, RoutedEventArgs e)
        {
            mySqlConn.Close();
            mySqlConn.Open();
            string selectQuery = "SELECT register_id, id_user, email FROM users WHERE login = @login ;";
            MySqlCommand mySqlComm = new MySqlCommand(selectQuery, mySqlConn);
            mySqlComm.Parameters.AddWithValue("@login", userName);

            MySqlDataReader mySqlReader = null;
            mySqlReader = mySqlComm.ExecuteReader();

            //string registerCode = "";

            if (mySqlReader.Read())
            {
                regCode = mySqlReader.GetString(0);
                userEmail = mySqlReader.GetString(2);
                if (regCode != "")
                {
                    var emailSender = new EmailSender();
                    emailSender.EmailSended += OnEmaiSended;
                    emailSender.SendEmail(userEmail, "Gold Chat: Resended Register Code", "Here is your activation code: " + regCode);
                }
            }
            mySqlReader.Close();
            mySqlConn.Close();
        }

        private void OnEmaiSended(object source, EmailSenderEventArgs args)
        {
            MessageBox.Show("Register Code resended to your email", "Close", MessageBoxButton.OK);
        }
    }
}

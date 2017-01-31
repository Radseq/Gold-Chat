using CommandClient;
using MySql.Data.MySqlClient;// for mysql
using System;
using System.Net; //for md5
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Gold
{
    //using Client;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class p_log : Window //typ
    {
        MySqlConnection cn;
        private string user_name;
        private string user_password;
        //private string userid;

        public Socket clientSocket;
        //public string strName;

        SpeechLib.SpVoice voice = new SpeechLib.SpVoice();

        public static string str = "";

        public string Password
        {
            get { return user_password; } //pass dekodowany
        }

        public string Client_Name
        {
            get { return loginTextBox.Text; }
        }

        /*public int Id
        {
            get { return int.Parse(userid); }
        }*/

        /*
         `user_id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
          `user_name` VARCHAR(255) NOT NULL,
          `user_password` VARCHAR(40) NOT NULL,
          `user_email` VARCHAR(255) NOT NULL,
         */

        private string server;
        private string dbHost;
        private string database;
        private string uid;
        private string password;
        //private int port;
        //private string s = "radseq.no-ip.org";
        //private int p = 5000;
        //private int t = 1000;

        public p_log()
        {
            InitializeComponent();
            loginTextBox.Focus();

            //cn = new MySqlConnection("server=db4free.net;uid=a9256518;pwd=atlandb;database=a587644;port=3306;");
            dbHost = Settings.DB_HOST;
            server = Settings.SERVER;
            database = Settings.DB;
            uid = Settings.DB_ROOT;
            password = Settings.DB_PASS;
            //port = Settings.DB_PORT;
            string connectionString = "SERVER=" + dbHost + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            cn = new MySqlConnection(connectionString);
            //TestConnection(Settings.HOST_NAME, Settings.PORT, 1000);

        }

        private void move(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation myDoubleAnimation1 = new DoubleAnimation();
            myDoubleAnimation1.From = 0.0;
            myDoubleAnimation1.To = 1.0;
            myDoubleAnimation1.Duration = new Duration(TimeSpan.FromSeconds(2));
            BeginAnimation(OpacityProperty, myDoubleAnimation1);
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

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            cn.Close();
            if (loginTextBox.Text != string.Empty && passwordBox.Password != string.Empty)
            {
                try
                {

                    /*user_name = loginTextBox.Text;
                    user_password = CalculateChecksum(passwordBox.Password);

                                // '" + user_name + "'  ' <-- musi byc na koncu... innaczej error
                    String Query = "SELECT * FROM users WHERE user_name = '" + user_name + "' and user_password = '" + user_password + "' ;";

                    MySqlCommand cmd = new MySqlCommand(Query, cn);
                    cn.Open();
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //uid = String.Format("{0}", reader[0]);
                            uid = reader.GetString(0);
                            name_from_db = reader.GetString(1);
                            password_from_db = reader.GetString(2);
                            //name_from_db = String.Format("{1}", reader[1]);
                            //password_from_db = String.Format("{2}", reader[2]);
                        }
                    }*/

                    user_name = loginTextBox.Text;
                    user_password = CalculateChecksum(passwordBox.Password);

                    // '" + user_name + "'  ' <-- musi byc na koncu... innaczej error
                    string Query = "SELECT register_id FROM users WHERE login = @userName AND password = @password ;";

                    MySqlCommand cmd = new MySqlCommand(Query, cn);
                    cmd.Parameters.AddWithValue("@userName", user_name);
                    cmd.Parameters.AddWithValue("@password", user_password);
                    cn.Open();


                    //MessageBox.Show("ServerVersion: " + cn.ServerVersion + "\nState: " + cn.State.ToString());

                    //object result = cmd.ExecuteScalar();


                    MySqlDataReader mySqlReader = null;
                    mySqlReader = cmd.ExecuteReader();

                    string registerCode = "";

                    if (mySqlReader.Read()) // If you're expecting only one line, change this to if(reader.Read()).
                    {
                        registerCode = mySqlReader.GetString(0);
                    }
                    mySqlReader.Close();
                    cn.Close();

                    //lepiej tak a wyjatki zostawic jak rzeczywiscie bedzie wyjatek (np brak polaczenia z baza)
                    if (registerCode != "")
                    {
                        //voice.Speak("wrong username Or password", SpeechLib.SpeechVoiceSpeakFlags.SVSFDefault);
                        //MessageBox.Show("Wrong login or password", "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                        register_code regCodeWindow = new register_code();
                        regCodeWindow.userName = Client_Name;
                        regCodeWindow.ShowDialog();

                        return;
                    }
                    else
                    {
                        // voice.Speak("Welcome back " + Client_Name.ToString(), SpeechLib.SpeechVoiceSpeakFlags.SVSFDefault);
                    }

                    //userid = result.ToString();

                    //masz id i jest zweryfikowane wiec ok




                    clientSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

                    //IPHostEntry ihe = Dns.GetHostEntry(Settings.HOST_NAME);
                    //IPAddress myself = ihe.AddressList[0];
                    //Server is listening on port 5000
                    // IPEndPoint ipEndPoint = new IPEndPoint(myself, 5000);
                    IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(server), Settings.PORT);

                    //Connect to the server
                    clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);


                    //cn.Close();
                    //this.DialogResult = true;
                    //cn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Unexpected error!{0}{1}", Environment.NewLine, ex.Message), "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
                MessageBox.Show("Fill in the fields", "Error validation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    clientSocket.EndSend(ar);
                    //Client_Name = tb_clientName.Text;
                    var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
                    anim.Completed += (s, _) => DialogResult = true;
                    BeginAnimation(OpacityProperty, anim);
                    //DialogResult = true;

                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Gold Chat", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                //We are connected so we login into the server
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    clientSocket.EndConnect(ar);

                    Data msgToSend = new Data();
                    msgToSend.cmdCommand = Command.Login;
                    msgToSend.strName = Client_Name;
                    msgToSend.strMessage = null;

                    byte[] bMessage = msgToSend.ToByte();

                    //Send the message to the server
                    clientSocket.BeginSend(bMessage, 0, bMessage.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Gold Chat", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to close Aplication?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                //oznaczenie ze ktos zamknal okno = chce zakonczyc aplikacje
                var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
                anim.Completed += (s, _) => DialogResult = false;
                BeginAnimation(OpacityProperty, anim);
            }
        }

        private void minimalizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            p_reg reg = new p_reg();
            reg.ShowDialog();
        }
    }
}
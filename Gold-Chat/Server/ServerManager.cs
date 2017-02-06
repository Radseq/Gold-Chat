using CommandClient;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Server
{
    public class ClientEventArgs : EventArgs
    {
        public string clientName { get; set; }
        public string clientMessageToSend { get; set; }
        public string clientMessageReciv { get; set; }
        public Socket clientSocket { get; set; }
        public string clientIpAdress { get; set; }
        public string clientPort { get; set; }
        public string clientEmail { get; set; }
        public int clientCommand { get; set; }
        public string clientFriendName { get; set; }

        //or just give this object public ServerManager ServerManager { get; set; }
    }

    //This class represents a client connected to server
    public class ServerManager
    {
        public event EventHandler<ClientEventArgs> ClientLogin;
        public event EventHandler<ClientEventArgs> ClientRegistration;
        public event EventHandler<ClientEventArgs> ClientReSendAckCode;
        public event EventHandler<ClientEventArgs> ClientLogout;
        public event EventHandler<ClientEventArgs> ClientMessage;
        public event EventHandler<ClientEventArgs> ClientList;
        public event EventHandler<ClientEventArgs> ClientSendMessage;
        public event EventHandler<ClientEventArgs> ClientReceiMessage;

        //For Database
        //private string server;
        private string dbHost;
        private string database;
        private string uid;
        private string password;

        MySqlConnection cn;

        private static ManualResetEvent allDone = new ManualResetEvent(false);

        //for  user
        //private string userName;
        //private string userPassword;
        //for registration
        //private string userPasswordConf;
        //private string userEmail;

        //list of all users
        private List<Client> clientList = new List<Client>();

        ServerLogger servLogger;
        byte[] byteData = new byte[1024];

        public ServerManager(ServerLogger servLogg)
        {
            dbHost = Settings.DB_HOST;
            //server = Settings.SERVER;
            database = Settings.DB;
            uid = Settings.DB_ROOT;
            password = Settings.DB_PASS;
            //port = Settings.DB_PORT;
            string connectionString = "SERVER=" + dbHost + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            cn = new MySqlConnection(connectionString);
            servLogger = servLogg;

        }

        internal void acceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            //conClient = new Client();
            Client conClient = new Client();
            conClient.cSocket = handler;
            conClient.addr = (IPEndPoint)handler.RemoteEndPoint;

            string acceptConnectrion = " >> Accept connection from client: " + conClient.addr.Address + " on Port: " + conClient.addr.Port;// + " Users Connected: " + clientList.Count;
            Console.WriteLine(acceptConnectrion);
            servLogger.msgLog(acceptConnectrion);
            clientList.Add(conClient); //When a user logs in to the server then we add her to our list of clients

            handler.BeginReceive(byteData, 0, byteData.Length, 0, new AsyncCallback(OnReceive), conClient);
        }

        internal void getConnection(Socket sock)
        {
            allDone.Reset();
            sock.BeginAccept(new AsyncCallback(acceptCallback), sock);
            allDone.WaitOne();
        }

        public void OnSend(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndSend(ar);
            }
            catch (Exception ex)
            {
                servLogger.msgLog(ex.Message);
            }
        }

        private void chnageUserPassword(ref Client conClient, Data msgReceived, ref Data msgToSend)
        {
            string oldPassword = "";
            string newPassword = msgReceived.strMessage;
            string Query = "SELECT password FROM users WHERE login = @userName ;";

            MySqlCommand cmd = new MySqlCommand(Query, cn);
            cmd.Parameters.AddWithValue("@userName", conClient.strName);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = cmd.ExecuteReader();

            if (mySqlReader.Read()) // If you're expecting only one line, change this to if(reader.Read()).
            {
                oldPassword = mySqlReader.GetString(0);
                if (oldPassword != "")
                {
                    if (oldPassword != newPassword)
                    {
                        string updateQuery = "UPDATE users SET password = @pass WHERE login = @login AND password = @oldPass ;";
                        MySqlCommand mySqlCommUpdate = new MySqlCommand(updateQuery, cn);

                        mySqlCommUpdate.Parameters.AddWithValue("@pass", newPassword);
                        mySqlCommUpdate.Parameters.AddWithValue("@login", conClient.strName);
                        mySqlCommUpdate.Parameters.AddWithValue("@oldPass", oldPassword);

                        if (mySqlCommUpdate.ExecuteNonQuery() > 0)
                        {
                            msgToSend.strMessage = "Your Password has been changed!";
                        }
                        else
                        {
                            msgToSend.strMessage = "Unknow Error while changing password";
                        }
                    }
                    else
                        msgToSend.strMessage = "New and old password are same!";
                }
                //else
                //msgToSend.strMessage = "Error when"; //????
            }
            else
            {
                msgToSend.strMessage = "Wrong login"; //????
            }
            mySqlReader.Close();
            cn.Close();
        }

        private void clientLogin(ref Client conClient, Data msgReceived, ref Data msgToSend)
        {
            string userName = msgReceived.strName;
            string userPassword = msgReceived.strMessage;
            string loginNotyfiUser = msgReceived.strMessage2;

            conClient.strName = userName;

            string Query = "SELECT register_id, email FROM users WHERE login = @userName AND password = @password ;";

            MySqlCommand cmd = new MySqlCommand(Query, cn);
            cmd.Parameters.AddWithValue("@userName", userName);
            cmd.Parameters.AddWithValue("@password", userPassword);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = cmd.ExecuteReader();

            string registerCode = "";
            string userEmail = ""; //used for send email notyfication on login to user 

            if (mySqlReader.Read()) // If you're expecting only one line, change this to if(reader.Read()).
            {
                registerCode = mySqlReader.GetString(0);
                userEmail = mySqlReader.GetString(1);
                if (registerCode != "")
                {
                    // user wont send activation code
                    msgToSend.cmdCommand = Command.ReSendEmail;
                    msgToSend.strMessage = "You must activate your account first.";
                }
                else
                {
                    //all is correct so user can use app
                    msgToSend.strMessage = "You are succesfully Log in";

                    if (loginNotyfiUser == "1") //user wants to be notyficated when login on account
                    {
                        var emailSender = new EmailSender();
                        emailSender.EmailSended += OnEmaiNotyficationLoginSended;
                        emailSender.SendEmail(userName, userEmail, "Gold Chat: Login Notyfication", "You have login: " + DateTime.Now.ToString("dd:MM On HH:mm:ss") + " To Gold Chat Account.");
                    }

                    OnClientLogin(userName, conClient.addr.Address.ToString(), conClient.addr.Port.ToString()); //server OnClientLogin occur only when succes program.cs -> OnClientLogin
                }
            }
            else
            {
                msgToSend.strMessage = "Wrong login or password";
            }
            mySqlReader.Close();
            cn.Close();
        }

        private void clientRegistration(Data msgReceived, ref Data msgToSend)
        {
            string userName = msgReceived.strName;
            string userPassword = msgReceived.strMessage;
            string userEmail = msgReceived.strMessage2;

            MySqlCommand mySqlComm = new MySqlCommand("", cn);
            cn.Open();
            mySqlComm.CommandText = "SELECT login, email, register_id FROM users WHERE login = @login and email = @email";
            mySqlComm.Parameters.AddWithValue("@login", userName);
            mySqlComm.Parameters.AddWithValue("@email", userEmail);

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
            cn.Close();


            if (loginExsist == userName)
            {
                msgToSend.strMessage = "Your login exists, try other one";
            }
            else if (emailExsist == userEmail)
            {
                msgToSend.strMessage = "Your email exists, try other one";
            }
            else if (registerCode != "")
            {
                msgToSend.strMessage = "You have already register, go to login windows and paste register key";
            }
            else
            {
                mySqlComm.CommandText = "INSERT INTO users (login, password, email, register_id) " + "VALUES (@user_name, @user_password, @user_email, @register_id)";

                string registrationCode = CalculateChecksum(userEmail);

                mySqlComm.Parameters.AddWithValue("@user_name", userName);
                mySqlComm.Parameters.AddWithValue("@user_password", userPassword);
                mySqlComm.Parameters.AddWithValue("@user_email", userEmail);
                mySqlComm.Parameters.AddWithValue("@register_id", registrationCode);

                cn.Open();
                if (mySqlComm.ExecuteNonQuery() > 0)
                {
                    var emailSender = new EmailSender();
                    emailSender.EmailSended += OnEmaiSended;

                    string emailMessage = string.Format(@"
<p>Witaj <strong>{0}</strong>.
    <br />Dziękujemy za rejestrację w aplikacji <strong>Gold Chat</strong>.
    <br />Zanim będziesz mógł kożystać z aplikacji, musisz wykonać ostatnia operacje.
    <br />Pamiętaj - musisz to zrobić zanim staniesz sie w pełni zarejestrowanym użytkownikiem.<br />
    <span style='text-decoration: underline;'>
        <em>Jedyne co musisz zrobić to skopiować kod aktywacyjny, oraz wkleić go w oknie <strong>Register Code</strong> okno to pojawi się gdy wpiszesz swój login i hasło w <strong>Oknie Logowania!.</strong></em>
    </span>
</p>
<p><br />
    A o to twój kod aktywacyjny : <span style='color: #ff0000;'><strong>{1}</strong></span>
</p>
<p>
    <strong>
    <span style='color: #ff0000;'> Pamiętaj by dokłanie skopiować KOD.</span>
    </strong>
</p>
<p>Dziękujemy <br /> Administracja Gold Chat.</p>", userName, registrationCode);


                    emailSender.SendEmail(userName, userEmail, "Gold Chat: Registration", emailMessage);

                    msgToSend.strMessage = "You has been registered";

                    cn.Close();
                }
                else
                {
                    msgToSend.strMessage = "Account NOT created with unknown reason.";
                }
            }
            OnClientRegistration(userName, userEmail);
        }

        private void clientReSendActivCode(Data msgReceived, ref Data msgToSend)
        {
            string userName = msgReceived.strName;
            string userEmail = "";
            string regCode = "";

            string userRegisterCode = msgReceived.strMessage;

            if (userRegisterCode != null)
            {

                cn.Close();
                cn.Open();
                string selectQuery = "SELECT register_id, email FROM users WHERE register_id = @register_id AND login = @login ;";
                MySqlCommand mySqlComm = new MySqlCommand(selectQuery, cn);
                mySqlComm.Parameters.AddWithValue("@register_id", userRegisterCode);
                mySqlComm.Parameters.AddWithValue("@login", userName);

                MySqlDataReader mySqlReader = null;
                mySqlReader = mySqlComm.ExecuteReader();

                if (mySqlReader.Read())
                {
                    regCode = mySqlReader.GetString(0);
                    userEmail = mySqlReader.GetString(1);
                }
                mySqlReader.Close();

                if (regCode == userRegisterCode)
                {
                    string updateQuery = "UPDATE users SET register_id = @reg_id WHERE email = @email ;";
                    MySqlCommand mySqlCommUpdate = new MySqlCommand(updateQuery, cn);

                    mySqlCommUpdate.Parameters.AddWithValue("@reg_id", "");
                    mySqlCommUpdate.Parameters.AddWithValue("@email", userEmail);

                    if (mySqlCommUpdate.ExecuteNonQuery() > 0)
                        msgToSend.strMessage = "Now you can login in to application";
                    else
                        msgToSend.strMessage = "Error when Activation contact to support";

                    cn.Close();
                }
                else
                    msgToSend.strMessage = "Activation code not match.";

                cn.Close();
            }
            else
            {
                cn.Open();
                string selectQuery = "SELECT register_id, email FROM users WHERE login = @login ;";
                MySqlCommand mySqlComm = new MySqlCommand(selectQuery, cn);
                mySqlComm.Parameters.AddWithValue("@login", userName);

                MySqlDataReader mySqlReader = null;
                mySqlReader = mySqlComm.ExecuteReader();

                if (mySqlReader.Read())
                {
                    regCode = mySqlReader.GetString(0);
                    userEmail = mySqlReader.GetString(1);
                    if (regCode != "")
                    {
                        var emailSender = new EmailSender();
                        emailSender.EmailSended += OnEmaiReSended;
                        emailSender.SendEmail(userName, userEmail, "Gold Chat: Resended Register Code", "Here is your activation code: " + regCode);

                        msgToSend.strMessage = "Activation code resended.";
                    }
                    else
                    {
                        msgToSend.strMessage = "You must activate an account.";
                    }
                }
                mySqlReader.Close();
                cn.Close();
                OnClientReSendAckCode(userName, userEmail);
            }
        }

        //using as logout and when client crash/internet disconect etc
        //return name to use with client crashed
        private string clientLogout(ref Client conClient, Data msgToSend, bool isClientCrash = false)
        {
            //When a user wants to log out of the server then we search for her 
            //in the list of clients and close the corresponding connection
            int nIndex = 0;
            foreach (Client client in clientList)
            {
                if (client.cSocket == conClient.cSocket)
                {
                    clientList.RemoveAt(nIndex);
                    if (isClientCrash) msgToSend.strName = client.strName;
                    break;
                }
                ++nIndex;
            }

            msgToSend.strMessage = "<<<" + conClient.strName + " has left the room>>>";
            OnClientLogout(conClient.strName, conClient.cSocket);

            conClient.cSocket.Close();

            return conClient.strName;
        }

        private void clientMessage(Data msgReceived, Data msgToSend)
        {
            msgToSend.strMessage = msgReceived.strName + ": " + msgReceived.strMessage;
            OnClientMessage(msgToSend.strMessage, msgReceived.strName + ": " + msgReceived.strMessage);
        }

        private void sendClientList(ref Client conClient, Data msgToSend)
        {
            msgToSend.cmdCommand = Command.List;
            msgToSend.strName = null;
            msgToSend.strMessage = null;

            //Collect the names of the user in the chat room
            foreach (Client client in clientList)
            {
                //To keep things simple we use asterisk as the marker to separate the user names
                msgToSend.strMessage += client.strName + "*";
            }

            byte[] message = msgToSend.ToByte();

            //Send the name of the users in the chat room
            conClient.cSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(OnSend), conClient.cSocket);
            OnClientList(msgToSend.strMessage);
        }

        private void OnReceive(IAsyncResult ar)
        {
            // Retrieve the client and the handler socket  
            // from the asynchronous client.  
            Client client = (Client)ar.AsyncState; // bad idea but catch need to see client, or maybe not bad idea because object is created and catch will not occur on constructor of class

            try
            {
                //Transform the array of bytes received from the user into an
                //intelligent form of object Data
                Data msgReceived = new Data(byteData);

                //We will send this object in response the users request
                Data msgToSend = new Data();

                //If the message is to login, logout, or simple text message
                //then when send to others the type of the message remains the same
                msgToSend.cmdCommand = msgReceived.cmdCommand;
                msgToSend.strName = msgReceived.strName;
                msgToSend.strMessage = msgReceived.strMessage;

                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:
                        clientLogin(ref client, msgReceived, ref msgToSend);
                        byte[] message = msgToSend.ToByte();
                        SendMessageLogin(ref client, message);
                        break;

                    case Command.Reg:
                        clientRegistration(msgReceived, ref msgToSend);
                        break;

                    case Command.changePassword:
                        chnageUserPassword(ref client, msgReceived, ref msgToSend);
                        break;

                    case Command.ReSendEmail:
                        clientReSendActivCode(msgReceived, ref msgToSend);
                        break;

                    case Command.Logout:
                        clientLogout(ref client, msgToSend);
                        break;

                    case Command.Message: //Text of the message that we will broadcast to all users
                        clientMessage(msgReceived, msgToSend);
                        break;

                    case Command.privMessage:
                        SendMessage(ref client, msgReceived, msgToSend, true);
                        break;

                    case Command.List:  //Send the names of all users in the chat room to the new user
                        sendClientList(ref client, msgToSend);
                        break;
                }

                if (msgToSend.cmdCommand != Command.List && msgToSend.cmdCommand != Command.privMessage) //List messages are not broadcasted
                {
                    SendMessage(ref client, msgReceived, msgToSend);
                }

                ReceivedMessage(ref client, msgReceived, byteData);
            }
            catch (Exception ex)
            {
                //so we make sure that client with got crash or internet close, server will send log out message
                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Logout;

                string exMessage = ("client: " + clientLogout(ref client, msgToSend, true) + " " + ex.Message);
                Console.WriteLine(exMessage);
                servLogger.msgLog(exMessage);

                SendMessage(ref client, null, msgToSend);

                //if (client is IDisposable) ((IDisposable)client).Dispose(); //free client
            }
        }

        private void SendMessageLogin(ref Client conClient, byte[] message)
        {
            conClient.cSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(OnSend), conClient.cSocket);
        }

        private void SendMessage(ref Client conClient, Data msgReceived, Data msgToSend, bool isPrivateMessage = false)
        {
            byte[] message = msgToSend.ToByte();

            foreach (Client cInfo in clientList)
            {
                if (isPrivateMessage ? (cInfo.strName == msgReceived.strMessage2 || msgReceived.strName == cInfo.strName)
                    : (cInfo.cSocket != conClient.cSocket || msgToSend.cmdCommand != Command.Login))
                {
                    //Send the message to all users, if private message send to sender and friend
                    if (msgToSend.strMessage == "You are succesfully Log in") //if we got msg from other users thats they login as user (conClient) will see this msg below
                    {
                        msgToSend.strMessage = "<<<" + msgReceived.strName + " has joined the room>>>";
                        message = msgToSend.ToByte();
                    }
                    cInfo.cSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(OnSend), cInfo.cSocket);
                }
            }
            if (isPrivateMessage == false)
                OnClientSendMessage(msgToSend.strMessage); //server will not see private messages 
        }

        private void ReceivedMessage(ref Client conClient, Data msgReceived, byte[] byteData)
        {
            if (msgReceived.cmdCommand != Command.Logout)
            {
                // conClient.cSocket.Receive(byteData, byteData.Length, SocketFlags.None);
                conClient.cSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), conClient);
            }
            else if (msgReceived.strMessage != null)// i want to see messages, messages will be null on login/logout
            {
                OnClientReceiMessage((int)msgReceived.cmdCommand, msgReceived.strName, msgReceived.strMessage, msgReceived.strMessage);
            }
        }

        protected virtual void OnClientLogin(string cName, string cIpadress, string cPort)
        {
            ClientLogin?.Invoke(this, new ClientEventArgs() { clientName = cName, clientIpAdress = cIpadress, clientPort = cPort });
        }

        protected virtual void OnClientRegistration(string cName, string cEmail)
        {
            ClientRegistration?.Invoke(this, new ClientEventArgs() { clientName = cName, clientEmail = cEmail });
        }

        protected virtual void OnClientReSendAckCode(string cName, string cEmail)
        {
            ClientReSendAckCode?.Invoke(this, new ClientEventArgs() { clientName = cName, clientEmail = cEmail });
        }

        protected virtual void OnClientLogout(string cName, Socket socket)
        {
            ClientLogout?.Invoke(this, new ClientEventArgs() { clientName = cName, clientSocket = socket });
        }

        protected virtual void OnClientMessage(string cMessageToSend, string cMessageRecev)
        {
            ClientMessage?.Invoke(this, new ClientEventArgs() { clientMessageToSend = cMessageToSend, clientMessageReciv = cMessageRecev });
        }
        protected virtual void OnClientList(string cMessage)
        {
            ClientList?.Invoke(this, new ClientEventArgs() { clientMessageToSend = cMessage });
        }
        protected virtual void OnClientSendMessage(string cMessage) //brodcasted messages
        {
            ClientSendMessage?.Invoke(this, new ClientEventArgs() { clientMessageToSend = cMessage });// do zrobienia cale data a nie tylko msgMessage
        }
        protected virtual void OnClientReceiMessage(int command, string cName, string cMessage, string cFriendName)
        {
            ClientReceiMessage?.Invoke(this, new ClientEventArgs() { clientCommand = command, clientName = cName, clientMessageReciv = cMessage, clientFriendName = cFriendName });// tu jeszcze nie wiem
        }

        private void OnEmaiSended(object source, EmailSenderEventArgs args)
        {
            string outStr = "Activation Code has been send to " + args.UserNameEmail + " email";
            servLogger.msgLog(outStr);
        }

        private void OnEmaiReSended(object source, EmailSenderEventArgs args)
        {
            string outStr = "Register Code resended to " + args.UserNameEmail + " email";
            servLogger.msgLog(outStr);
        }

        private void OnEmaiNotyficationLoginSended(object sender, EmailSenderEventArgs e)
        {
            string outStr = "Login Notyfication to " + e.UserNameEmail + " email";
            servLogger.msgLog(outStr);
        }

        private static string CalculateChecksum(string inputString)
        {
            var md5 = new MD5CryptoServiceProvider();
            var hashbytes = md5.ComputeHash(Encoding.UTF8.GetBytes(inputString));
            var hashstring = "";
            foreach (var hashbyte in hashbytes)
                hashstring += hashbyte.ToString("x2");

            return hashstring;
        }
    }
}

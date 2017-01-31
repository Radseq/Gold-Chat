using CommandClient;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

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

        //for  user
        //private string userName;
        //private string userPassword;
        //for registration
        //private string userPasswordConf;
        //private string userEmail;

        //list of all users
        //List<Client> clientList;

        ServerLogger servLogger;

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

        byte[] message;

        //public Client clientInfo;

        public void chnageUserPassword(Client clientInfo, Data msgReceived, ref Data msgToSend)
        {
            string oldPassword = "";
            string newPassword = msgReceived.strMessage;
            string Query = "SELECT password FROM users WHERE login = @userName ;";

            MySqlCommand cmd = new MySqlCommand(Query, cn);
            cmd.Parameters.AddWithValue("@userName", clientInfo.strName);
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
                        mySqlCommUpdate.Parameters.AddWithValue("@login", clientInfo.strName);
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

        public void clientLogin(ref Client clientInfo, Data msgReceived, ref Data msgToSend)
        {
            string userName = msgReceived.strName;
            string userPassword = msgReceived.strMessage;
            string loginNotyfiUser = msgReceived.strMessage2;

            clientInfo.strName = userName;

            string clientIpAdress = "";
            string clientPort = "";

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
                    // no i urzytkownik nie wyslal kodu aktywacji
                    msgToSend.cmdCommand = Command.ReSendEmail;
                    msgToSend.strMessage = "You must activate your account first.";
                }
                else
                {
                    //wszystko jest dobrze wiec urzytkownik powinien korzytacj z aplikacji
                    msgToSend.strMessage = "You are succesfully Log in";

                    if (loginNotyfiUser == "1") //user wants to be notyficated when login on account
                    {
                        var emailSender = new EmailSender();
                        emailSender.EmailSended += OnEmaiNotyficationLoginSended;
                        emailSender.SendEmail(userName, userEmail, "Gold Chat: Login Notyfication", "You have login: " + DateTime.Now.ToString("dd:MM On HH:mm:ss") + " To Gold Chat Account.");
                    }

                    clientIpAdress = ((IPEndPoint)clientInfo.cSocket.RemoteEndPoint).Address.ToString();
                    clientPort = ((IPEndPoint)clientInfo.cSocket.RemoteEndPoint).Port.ToString();
                }
            }
            else
            {
                msgToSend.strMessage = "Wrong login or password";
            }
            mySqlReader.Close();
            cn.Close();
            OnClientLogin(userName, clientIpAdress, clientPort);
        }

        public void clientRegistration(Data msgReceived, ref Data msgToSend)
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

        public void clientReSendActivCode(Data msgReceived, ref Data msgToSend)
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
                    {
                        msgToSend.strMessage = "Now you can login in to application";
                    }
                    else
                    {
                        msgToSend.strMessage = "Error when Activation contact to support";
                    }
                    cn.Close();
                }
                else
                {
                    msgToSend.strMessage = "Activation code not match.";
                }
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
        public string clientLogout(ref List<Client> clientList, ref Client clientInfo, /*Data msgReceived,*/ Data msgToSend, bool isClientCrash = false)
        {
            //When a user wants to log out of the server then we search for her 
            //in the list of clients and close the corresponding connection
            int nIndex = 0;
            foreach (Client client in clientList)
            {
                if (client.cSocket == clientInfo.cSocket)
                {
                    clientList.RemoveAt(nIndex);
                    if (isClientCrash) msgToSend.strName = client.strName;
                    break;
                }
                ++nIndex;
            }

            clientInfo.cSocket.Close();

            msgToSend.strMessage = "<<<" + clientInfo.strName + " has left the room>>>";
            OnClientLogout(clientInfo.strName, clientInfo.cSocket);
            return clientInfo.strName;
        }

        public void clientMessage(Data msgReceived, Data msgToSend)
        {
            msgToSend.strMessage = msgReceived.strName + ": " + msgReceived.strMessage;
            OnClientMessage(msgToSend.strMessage, msgReceived.strName + ": " + msgReceived.strMessage);
        }

        public void sendClientList(List<Client> clientList, Client clientInfo, Data msgToSend)
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

            message = msgToSend.ToByte();

            //Send the name of the users in the chat room
            clientInfo.cSocket.Send(message, 0, message.Length, SocketFlags.None);
            OnClientList(msgToSend.strMessage);
        }

        public void SendMessage(List<Client> clientList, Client clientInfo, Data msgReceived, Data msgToSend, bool isPrivateMessage = false)
        {
            message = msgToSend.ToByte();

            foreach (Client cInfo in clientList)
            {
                //if (isPrivateMessage ? cInfo.strName == msgReceived.strMessage : cInfo.cSocket != clientInfo.cSocket || msgToSend.cmdCommand != Command.Login)
                //{
                //Send the message to all users, or to friend
                //if (isPrivateMessage && cInfo.strName == msgReceived.strMessage)
                cInfo.cSocket.Send(message, 0, message.Length, SocketFlags.None);
                //if (isPrivateMessage == true && cInfo.strName == msgReceived.strMessage)
                //   break;
                //}
            }
            if (isPrivateMessage == false)
                OnClientSendMessage(msgToSend.strMessage); //server will not see private messages 
        }

        /*public void SendMessage(Client clientInfo, DataLogin msgReceived, DataLogin msgToSend)
        {
            message = msgToSend.ToByte();
            clientInfo.cSocket.Send(message, 0, message.Length, SocketFlags.None);
            OnClientSendMessage(msgToSend.strMessage);
        }*/

        public void ReceivedMessage(Client clientInfo, Data msgReceived, byte[] byteData)
        {
            if (msgReceived.cmdCommand != Command.Logout)
            {
                clientInfo.cSocket.Receive(byteData, byteData.Length, SocketFlags.None);
            }
            else if (msgReceived.strMessage != null)// i want to see messages, messages will be null on login/logout
            {
                OnClientReceiMessage((int)msgReceived.cmdCommand, msgReceived.strName, msgReceived.strMessage, msgReceived.strMessage);
            }
        }
        /*
        public void ReceivedMessage(Client clientInfo, DataLogin msgReceived, byte[] byteData)
        {
            clientInfo.cSocket.Receive(byteData, byteData.Length, SocketFlags.None);
            OnClientReceiMessage((int)msgReceived.cmdCommand, msgReceived.strMessage, msgReceived.strMessage, msgReceived.loginName);
        }*/

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

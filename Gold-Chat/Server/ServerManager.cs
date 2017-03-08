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
        public string clientMessageTwoToSend { get; set; }
        public string clientMessageReciv { get; set; }
        public Socket clientSocket { get; set; }
        public string clientIpAdress { get; set; }
        public string clientPort { get; set; }
        public string clientEmail { get; set; }
        public int clientCommand { get; set; }
        public string clientFriendName { get; set; }
        public string clientChannelName { get; set; }
        public string clientNameChannel { get; set; } //client name with joined to chanel

        //or just give this object public ServerManager ServerManager { get; set; }
    }

    //This class represents a client connected to server
    /// <summary>
    /// I THINK:
    /// NOTE I USE ALL THE TIME DB WITH IS BAD, WE SHOULD GET VALUES FROM DB AT START USER AND MANAGE THEM IN TIME OF CONNECTION
    /// </summary>

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
        //channel
        public event EventHandler<ClientEventArgs> ClientCreateChannel;
        public event EventHandler<ClientEventArgs> ClientJoinChannel;
        public event EventHandler<ClientEventArgs> ClientDeleteChannel;
        public event EventHandler<ClientEventArgs> ClientExitChannel; //exit
        public event EventHandler<ClientEventArgs> ClientEditChannel; //        edit
        public event EventHandler<ClientEventArgs> ClientChannelMessage;
        //friend
        public event EventHandler<ClientEventArgs> ClientAddFriend;
        public event EventHandler<ClientEventArgs> ClientDeleteFriend;

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

        //The collection of all clients logged into the room
        private List<Client> clientList = new List<Client>();
        //list of all channels
        private List<Channel> channels = new List<Channel>();

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

            getChannelsFromDB();
        }

        private void getChannelsFromDB()
        {
            string Query = "SELECT channel_name,id_user_founder FROM channel";
            MySqlCommand mySqlComm = new MySqlCommand(Query, cn);
            //mySqlComm.Parameters.AddWithValue("@idUser", conClient.id);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = mySqlComm.ExecuteReader();

            int numberOfChannel = 0;
            int founderUser = 0;
            string channelName = "";

            while (mySqlReader.Read())
            {
                channelName = mySqlReader.GetString(numberOfChannel);
                founderUser = mySqlReader.GetInt16(numberOfChannel + 1);

                Channel channel = new Channel(mySqlReader.GetString(numberOfChannel), founderUser);
                if (!channels.Contains(channel))
                    channels.Add(channel);


                //foreach (Channel ch in channels)
                //{
                //    if (ch.ChannelName != channelName)
                //    {
                //        Channel channel = new Channel(mySqlReader.GetString(numberOfChannel), founderUser);
                //        channels.Add(channel);
                //    }
                //}
                numberOfChannel++;
            }

            mySqlReader.Close();
            cn.Close();
        }

        internal void acceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

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

        private void chnageUserPassword(Client conClient, Data msgReceived, ref Data msgToSend)
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
                msgToSend.strMessage = "Wrong login";
            }
            mySqlReader.Close();
            cn.Close();
        }

        private string CheckUserBan(int id_user)
        {
            string Query = "SELECT end_ban FROM user_bans WHERE id_user = @IdUser";

            MySqlCommand cmd = new MySqlCommand(Query, cn);
            cmd.Parameters.AddWithValue("@IdUser", id_user);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = cmd.ExecuteReader();

            string endBanDateFromDB = "";

            if (mySqlReader.Read()) //Expecting only one line
            {
                endBanDateFromDB = mySqlReader.GetString(0);

                DateTime dt1 = DateTime.Parse(endBanDateFromDB);
                DateTime dt2 = DateTime.Now;

                if (dt1.Date > dt2.Date)
                {
                    return endBanDateFromDB;
                }
                else
                {
                    return null;
                }
            }
            mySqlReader.Close();
            cn.Close();
            return null;
        }

        private void clientLogin(ref Client conClient, Data msgReceived, ref Data msgToSend)
        {
            string userName = msgReceived.strName;
            string userPassword = msgReceived.strMessage;
            string loginNotyfiUser = msgReceived.strMessage2;

            string Query = "SELECT register_id, email, id_user, login, permission FROM users WHERE login = @userName AND password = @password";

            MySqlCommand cmd = new MySqlCommand(Query, cn);
            cmd.Parameters.AddWithValue("@userName", userName);
            cmd.Parameters.AddWithValue("@password", userPassword);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = cmd.ExecuteReader();

            string registerCode = "";
            string userEmail = ""; //used for send email notyfication on login to user 
            string userLogin = ""; //used to eliminate username problem extample: login:radseq db: Radseq, i dont know why select working

            if (mySqlReader.Read()) //Expecting only one line
            {
                registerCode = mySqlReader.GetString(0);
                userEmail = mySqlReader.GetString(1);
                userLogin = mySqlReader.GetString(3);
                if (userName != userLogin)
                    msgToSend.strMessage = "Wrong login or password";
                else if (registerCode != "")
                {
                    // user wont send activation code
                    msgToSend.cmdCommand = Command.ReSendEmail;
                    msgToSend.strMessage = "You must activate your account first.";
                }
                else
                {
                    conClient.id = mySqlReader.GetInt16(2); //give id from db
                    conClient.permission = mySqlReader.GetInt16(4);
                    cn.Close();

                    string ban = CheckUserBan(conClient.id);
                    if (ban == null)
                    {
                        //all is correct so user can use app
                        conClient.strName = userLogin;
                        msgToSend.strMessage = "You are succesfully Log in";

                        conClient.enterChannels = new List<string>(); // init of channels whitch i joined
                        conClient.ignoredUsers = new List<string>(); // init of ignored users

                        if (loginNotyfiUser == "1") //user wants to be notyficated when login on account
                        {
                            var emailSender = new EmailSender();
                            emailSender.EmailSended += OnEmaiNotyficationLoginSended;
                            emailSender.SendEmail(userName, userEmail, "Gold Chat: Login Notyfication", "You have login: " + DateTime.Now.ToString("dd:MM On HH:mm:ss") + " To Gold Chat Account.");
                        }

                        OnClientLogin(userName, conClient.addr.Address.ToString(), conClient.addr.Port.ToString()); //server OnClientLogin occur only when succes program.cs -> OnClientLogin
                    }
                    else msgToSend.strMessage = "You are banned untill " + ban;
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
                mySqlComm.CommandText = "INSERT INTO users (login, password, email, register_id, permission) " + "VALUES (@user_name, @user_password, @user_email, @register_id, @perm)";

                string registrationCode = CalculateChecksum(userEmail);

                mySqlComm.Parameters.AddWithValue("@user_name", userName);
                mySqlComm.Parameters.AddWithValue("@user_password", userPassword);
                mySqlComm.Parameters.AddWithValue("@user_email", userEmail);
                mySqlComm.Parameters.AddWithValue("@register_id", registrationCode);
                mySqlComm.Parameters.AddWithValue("@perm", 0); //give permission to 0 for all users

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
                        msgToSend.strMessage = "You must activate an account.";
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

                foreach (Channel ch in channels) //dont need send list etc to user because all channel got msg that user log out and channel check if exists in theirs list
                {
                    if (ch.Users.Contains(client.strName))
                        ch.Users.Remove(client.strName);
                }
            }

            msgToSend.strMessage = conClient.strName;
            OnClientLogout(conClient.strName, conClient.cSocket);

            conClient.cSocket.Close();

            return conClient.strName;
        }

        private void clientMessage(Data msgReceived, Data msgToSend)
        {
            msgToSend.strMessage = msgReceived.strName + ": " + msgReceived.strMessage;
            OnClientMessage(msgToSend.strMessage, msgReceived.strName + ": " + msgReceived.strMessage);
        }
        /// <summary>
        /// Sending list of logged users 
        /// adam*bob*matty
        /// </summary>
        /// <param name="conClient">Connected User to server</param>
        /// <param name="msgToSend">Respond to Client</param>
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
            OnClientList(msgToSend.strMessage, ""); //second parameter used for diffrent lists
            SendServerRespond(ref conClient, ref msgToSend);
        }
        /// <summary>
        /// Sending to the user the list of all channels
        /// </summary>
        /// <param name="conClient">User</param>
        /// <param name="msgToSend">List of channels</param>
        private void sendChannelList(Client conClient, ref Data msgToSend)
        {
            msgToSend.cmdCommand = Command.List;
            msgToSend.strName = null;
            msgToSend.strMessage = "Channel";
            msgToSend.strMessage2 = null;
            msgToSend.strMessage3 = null;
            msgToSend.strMessage4 = null;

            foreach (Channel ch in channels)
            {
                msgToSend.strMessage2 += ch.ChannelName + "*";
            }

            OnClientList(msgToSend.strMessage, msgToSend.strMessage2);
        }

        //send to user list of channel that he joined before
        private void sendChannelListJoined(Client conClient, ref Data msgToSend)
        {
            msgToSend.cmdCommand = Command.List;
            msgToSend.strName = null;
            //msgToSend.strMessage = "ChannelsJoined";

            //select user channels
            string Query = "SELECT c.channel_name FROM channel c, user_channel uc WHERE uc.id_channel = c.id_channel AND uc.id_user = @idUser";
            //string Query = "SELECT channel_name FROM channel";
            MySqlCommand mySqlComm = new MySqlCommand(Query, cn);
            mySqlComm.Parameters.AddWithValue("@idUser", conClient.id);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = mySqlComm.ExecuteReader();

            int numberOfChannels = 0;

            while (mySqlReader.Read())
            {
                msgToSend.strMessage2 += mySqlReader.GetString(numberOfChannels) + "*";
                /// if (!conClient.channels.Contains(mySqlReader.GetString(numberOfChannels)))
                //conClient.channels.Add(mySqlReader.GetString(numberOfChannels));
                numberOfChannels++;
            }

            mySqlReader.Close();
            cn.Close();

            OnClientList(msgToSend.strMessage, msgToSend.strMessage2);
        }
        //todo i send all names form db, what if someone is offline?
        private void sendFriendList(Client conClient, ref Data msgToSend)
        {
            msgToSend.cmdCommand = Command.List;
            msgToSend.strName = null;
            msgToSend.strMessage = "Friends";

            string Query = "SELECT u.login FROM users u, user_friend uf WHERE uf.id_friend = u.id_user AND uf.id_user = @idUser";

            MySqlCommand mySqlComm = new MySqlCommand(Query, cn);
            mySqlComm.Parameters.AddWithValue("@idUser", conClient.id);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = mySqlComm.ExecuteReader();

            int numberOfFriends = 0;

            while (mySqlReader.Read()) // If you're expecting only one line, change this to if(reader.Read()).
            {
                msgToSend.strMessage2 += mySqlReader.GetString(numberOfFriends) + "*";
                numberOfFriends++;
            }

            mySqlReader.Close();
            cn.Close();

            OnClientList(msgToSend.strMessage, msgToSend.strMessage2);
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
                msgToSend.strMessage2 = msgReceived.strMessage2;
                msgToSend.strMessage3 = msgReceived.strMessage3;
                msgToSend.strMessage4 = msgReceived.strMessage4;

                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:
                        clientLogin(ref client, msgReceived, ref msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        if (msgToSend.strMessage == "You are succesfully Log in") //if we got msg from other users thats they login as user (conClient) will see this msg below
                        {
                            msgToSend.strMessage = "<<<" + msgReceived.strName + " has joined the room>>>";
                            SendMessageToAll(ref client, msgToSend);
                        }
                        break;

                    case Command.Reg:
                        clientRegistration(msgReceived, ref msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        break;

                    case Command.changePassword:
                        chnageUserPassword(client, msgReceived, ref msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        break;

                    case Command.ReSendEmail:
                        clientReSendActivCode(msgReceived, ref msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        break;

                    case Command.Logout:
                        clientLogout(ref client, msgToSend);
                        SendMessageToAll(ref client, msgToSend);
                        break;

                    case Command.Message: //Text of the message that we will broadcast to all users
                        if (msgReceived.strMessage2 == null)
                            clientMessage(msgReceived, msgToSend);
                        else
                        {
                            msgToSend.strMessage = msgReceived.strName + ": " + msgReceived.strMessage;
                            msgToSend.strMessage2 = msgReceived.strMessage2;
                            OnClientChannelMessage(msgToSend.strMessage, msgReceived.strName + ": " + msgReceived.strMessage + " On:" + msgReceived.strMessage2);
                        }
                        SendMessageToAll(ref client, msgToSend);
                        break;

                    case Command.privMessage:
                        SendMessageToSomeone(ref client, msgReceived, msgToSend);
                        break;

                    case Command.createChannel:
                        channelCreate(client, msgReceived, ref msgToSend);
                        if (msgToSend.strMessage2 != "CreatedChannel")
                            SendServerRespond(ref client, ref msgToSend);
                        break;

                    case Command.joinChannel:
                        channelJoin(client, msgReceived, ref msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        break;

                    case Command.exitChannel:
                        channelExit(client, msgReceived, ref msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        break;

                    case Command.deleteChannel:
                        channelDelete(client, msgReceived, ref msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        break;
                    case Command.enterChannel:
                        channelEnter(client, msgReceived, ref msgToSend);
                        break;
                    case Command.leaveChannel:
                        channelLeave(ref client, msgReceived.strMessage, msgReceived.strName);
                        SendMessageToChannel(ref client, msgReceived.strMessage, ref msgToSend);
                        break;

                    case Command.List:  //Send the names of all users in the chat room to the new user
                        if (msgReceived.strMessage == "Channel")
                            sendChannelList(client, ref msgToSend);
                        else if (msgReceived.strMessage == "Friends")
                            sendFriendList(client, ref msgToSend);
                        else if (msgReceived.strMessage == "ChannelsJoined")
                            sendChannelListJoined(client, ref msgToSend);
                        else if (msgReceived.strMessage == "ChannelUsers") //send user with enter to channel list of users
                            sendListUsersInChannel(client, ref msgToSend, ref msgReceived);
                        else if (msgReceived.strMessage == "IgnoredUsers")
                            sendListIgnoredUsers(client, ref msgToSend, ref msgReceived);
                        else
                            sendClientList(ref client, msgToSend);
                        if (msgToSend.strMessage2 != null) SendServerRespond(ref client, ref msgToSend);
                        break;

                    case Command.manageFriend:
                        ManageUserFriend(client, msgReceived, msgToSend);
                        //SendServerRespond(ref client, msgToSend);
                        break;
                    case Command.ignoreUser:
                        ManageIgnoreUser(client, msgReceived, msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        break;
                    case Command.kick:
                        ClientKick(client, msgReceived, msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        //msgToSend.strName = friendName;
                        //SendMessageToNick(ref client, msgReceived, msgToSend);
                        break;
                    case Command.ban:
                        ClientBan(ref client, msgReceived, msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        break;
                    case Command.kickUserChannel:
                        ClientKickFromChannel(ref client, msgReceived, msgToSend);
                        break;
                    case Command.banUserChannel:
                        ClientBanFromChannel(ref client, msgReceived, msgToSend);
                        break;
                }

                //if (msgToSend.cmdCommand != Command.List && msgToSend.cmdCommand != Command.privMessage) //List messages are not broadcasted
                // {
                //    SendMessageToAll(ref client, msgReceived, msgToSend);
                //}

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

                SendMessageToAll(ref client, msgToSend);

                //if (client is IDisposable) ((IDisposable)client).Dispose(); //free client
            }
        }

        private void ClientBanFromChannel(ref Client client, Data msgReceived, Data msgToSend)
        {
            throw new NotImplementedException();
        }

        private void ClientKickFromChannel(ref Client client, Data msgReceived, Data msgToSend)
        {
            string userName = msgReceived.strMessage;       //nick of kicked user
            string kickReason = msgReceived.strMessage2;    //Reason of kick
            string channelName = msgReceived.strMessage3;

            //todo select if user with want ban another is creator of channel, and/or know admin passowrd

            //todo in client side get kick from channel ->
            //todo make tab list in program cs and when user got kick, close channel from tab list

            foreach (Channel ch in channels)
            {
                if (ch.ChannelName == channelName /*&& ch.FounderiD == client.id*/)
                {
                    if (ch.FounderiD == client.id)
                    {
                        if (ch.Users.Contains(userName))
                        {
                            SendMessageToChannel(ref client, channelName, ref msgToSend);
                            ch.Users.Remove(userName);

                            foreach (Client cInfo in clientList)
                            {
                                if (cInfo.strName == userName)
                                    if (cInfo.enterChannels.Contains(channelName))
                                    {
                                        cInfo.enterChannels.Remove(userName);
                                        //todo msg to client that got kicked form channel, when got this kind of message show msgbox
                                    }
                            }
                        }
                        else
                            msgToSend.strMessage2 = "There no " + userName + " in your channel";
                    }
                    else
                        msgToSend.strMessage2 = "Only channel founder can kick";
                }
                else
                    msgToSend.strMessage2 = "Your Channel not exists";
            }
        }


        private void ClientBan(ref Client client, Data msgReceived, Data msgToSend)
        {
            string userName = msgReceived.strMessage;
            string time = msgReceived.strMessage2;
            string banReason = msgReceived.strMessage3;

            if (client.permission > 0)
            {
                foreach (Client c in clientList)
                {
                    if (c.strName == userName && c.permission == 0)
                    {
                        string addQuery = "INSERT INTO user_bans (id_user, reason, end_ban) " +
       "VALUES (@idUser, @BanReason, @EndBanDateTime)";

                        MySqlCommand mySqlCommAdd = new MySqlCommand(addQuery, cn);

                        mySqlCommAdd.Parameters.AddWithValue("@idUser", c.id);
                        mySqlCommAdd.Parameters.AddWithValue("@BanReason", banReason);
                        mySqlCommAdd.Parameters.AddWithValue("@EndBanDateTime", time);
                        cn.Open();

                        if (mySqlCommAdd.ExecuteNonQuery() > 0)
                        {
                            SendMessageToAll(ref client, msgToSend);
                            c.cSocket.Close();
                        }
                        else
                            msgToSend.strMessage2 = "Cannot ban " + userName + " unknown reason";
                        cn.Close();
                    }
                }
            }
            else msgToSend.strMessage = "You dont have permission to kick " + userName;
        }

        private void ClientKick(Client client, Data msgReceived, Data msgToSend)
        {
            string userName = msgReceived.strMessage;      //nick of kicked user
            string kickReason = msgReceived.strMessage2;    //Reason of kick

            //todo select if user with want ban another is creator of channel, and/or know admin passowrd

            //todo in client side get kick from channel ->
            //todo make tab list in program cs and when user got kick, close channel from tab list

            if (client.permission > 0)
            {
                foreach (Client cInfo in clientList)
                {
                    if (cInfo.strName == userName && cInfo.permission == 0)
                    {
                        SendMessageToAll(ref client, msgToSend);
                        cInfo.cSocket.Close();
                    }
                }
            }
            else msgToSend.strMessage = "You cannot kick " + userName + " because you dont have permissions";

        }

        //todo check if user is in our friend list
        private void ManageIgnoreUser(Client client, Data msgReceived, Data msgToSend)
        {
            string type = msgReceived.strMessage;
            msgToSend.strMessage = msgReceived.strMessage;
            string userName = msgReceived.strMessage2;

            string Query = "SELECT id_user FROM users WHERE login = @IgnoreName";

            MySqlCommand mySqlComm = new MySqlCommand(Query, cn);
            mySqlComm.Parameters.AddWithValue("@IgnoreName", userName);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = mySqlComm.ExecuteReader();

            int idUser = 0;

            if (mySqlReader.Read())
            {
                idUser = mySqlReader.GetInt16(0);

                mySqlReader.Close();

                msgToSend.strMessage3 = userName;

                if (type == "AddIgnore")
                {
                    if (!client.ignoredUsers.Contains(userName))
                    {
                        string addQuery = "INSERT INTO user_ignored (id_user, id_user_ignored) " +
                               "VALUES (@idUser, @idUserIgnored)";

                        MySqlCommand mySqlCommAdd = new MySqlCommand(addQuery, cn);

                        mySqlCommAdd.Parameters.AddWithValue("@idUser", client.id);
                        mySqlCommAdd.Parameters.AddWithValue("@idUserIgnored", idUser);

                        if (mySqlCommAdd.ExecuteNonQuery() > 0)
                        {
                            msgToSend.strMessage2 = "You are now ignore: " + userName;
                            client.ignoredUsers.Add(userName);
                            //OnClientIgnoreUser(client.strName, friendName);
                        }
                        else
                            msgToSend.strMessage2 = "Cannot ignore " + userName + " unknown reason";
                    }
                    else
                        msgToSend.strMessage2 = "Cannot ignore " + userName + " because already ignored!";
                }
                else if (type == "DeleteIgnore")
                {
                    if (client.ignoredUsers.Contains(userName))
                    {
                        string delQuery = "DELETE FROM user_ignored WHERE id_user = @idUser AND id_user_ignored = @idUserIgnored";

                        MySqlCommand mySqlCommDel = new MySqlCommand(delQuery, cn);

                        mySqlCommDel.Parameters.AddWithValue("@idUser", client.id);
                        mySqlCommDel.Parameters.AddWithValue("@idUserIgnored", idUser);

                        if (mySqlCommDel.ExecuteNonQuery() > 0)
                        {
                            msgToSend.strMessage2 = "You are delete from ignore list user: " + userName;
                            client.ignoredUsers.Remove(userName);
                        }
                        else
                            msgToSend.strMessage2 = "Cannot delete ignore from " + userName + " unknown reason";
                    }
                    else
                        msgToSend.strMessage2 = "Cannot delete ignore from " + userName + " because not ignored!";
                }
                else msgToSend.strMessage2 = "There is only Add or Delete users option";

            }
            else msgToSend.strMessage2 = "There is no user that you want Add or Delete ignore.";
            cn.Close();
        }

        private void sendListIgnoredUsers(Client client, ref Data msgToSend, ref Data msgReceived)
        {
            msgToSend.cmdCommand = Command.List;
            msgToSend.strName = null;
            msgToSend.strMessage = "IgnoredUsers";

            string Query = "SELECT u.login FROM users u, user_ignored ui WHERE ui.id_user_ignored = u.id_user AND ui.id_user = @idUser";

            MySqlCommand mySqlComm = new MySqlCommand(Query, cn);
            mySqlComm.Parameters.AddWithValue("@idUser", client.id);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = mySqlComm.ExecuteReader();

            int numberIgnored = 0;

            while (mySqlReader.Read())
            {
                msgToSend.strMessage2 += mySqlReader.GetString(numberIgnored) + "*";
                client.ignoredUsers.Add(mySqlReader.GetString(numberIgnored));
                numberIgnored++;
            }

            mySqlReader.Close();
            cn.Close();

            OnClientList(msgToSend.strMessage, msgToSend.strMessage2);
        }

        //when user enter to channel, he send to server ask for users already in this channel
        private void sendListUsersInChannel(Client client, ref Data msgToSend, ref Data msgReceived)
        {
            //msgToSend.strMessage2 = msgReceived.strMessage2; // i should add this to begining of OnReceive

            foreach (Channel ch in channels)
            {
                if (ch.ChannelName == msgReceived.strMessage2)
                {
                    foreach (string user in ch.Users)
                    {
                        msgToSend.strMessage3 += user + "*";
                    }
                }
            }
        }

        private void channelLeave(ref Client client, string channelName, string userName)
        {
            //name of user and name of channel send to users in channels
            client.enterChannels.Remove(channelName);
            //remove user form channel users
            foreach (Channel ch in channels)
            {
                if (ch.ChannelName == channelName)
                {
                    ch.Users.Remove(userName);
                }
            }

            //OnClientLeaveChannel(channelName, client.strName); //todo
        }

        private void channelEnter(Client client, Data msgReceived, ref Data msgToSend)
        {
            string channelName = msgReceived.strMessage;

            string Query = "SELECT uc.id_channel,c.welcome_message FROM channel c, user_channel uc WHERE uc.id_channel = c.id_channel AND c.channel_name = @channelName AND uc.id_user = @idUser";

            MySqlCommand mySqlComm = new MySqlCommand(Query, cn);
            mySqlComm.Parameters.AddWithValue("@ChannelName", channelName);
            mySqlComm.Parameters.AddWithValue("@idUser", client.id);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = mySqlComm.ExecuteReader();

            int id_channel_db = 0;
            string motd = "";

            if (mySqlReader.Read()) //Expecting only one line
            {
                id_channel_db = mySqlReader.GetInt16(0);
                motd = mySqlReader.GetString(1);

                mySqlReader.Close();

                //msgToSend.strMessage = channelName;

                //add user to channel users
                foreach (Channel ch in channels)
                {
                    if (ch.ChannelName == channelName)
                    {
                        if (!ch.Users.Contains(msgReceived.strName) && (!client.enterChannels.Contains(channelName)))
                        {
                            ch.Users.Add(msgReceived.strName);
                            client.enterChannels.Add(channelName);

                            msgToSend.strMessage2 = "enter";
                            msgToSend.strMessage3 = motd;

                            SendMessageToChannel(ref client, channelName, ref msgToSend);
                        }
                        else
                        {
                            msgToSend.strMessage2 = "deny";
                            msgToSend.strMessage3 = "Cannot enter Because you already entered to " + channelName;
                            SendServerRespond(ref client, ref msgToSend);
                        }
                    }
                }

                //OnClientEnterChannel(channelName, client.strName); //todo
            }
            else
            {
                msgToSend.strMessage2 = "deny";
                msgToSend.strMessage3 = "Cannot Join Because you not join to " + channelName;
                SendServerRespond(ref client, ref msgToSend);
            }
            cn.Close();
        }

        //used in ManageUserFriend function
        private bool AddFriendToDb(MySqlConnection cn, int clientId, int friendId)
        {
            string Query = "INSERT INTO user_friend (id_user, id_friend) " +
                           "VALUES (@idUser, @idFriend)";

            MySqlCommand mySqlComm = new MySqlCommand(Query, cn);

            mySqlComm.Parameters.AddWithValue("@idUser", clientId);
            mySqlComm.Parameters.AddWithValue("@idFriend", friendId);

            if (mySqlComm.ExecuteNonQuery() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //used in ManageUserFriend function
        private bool DeleteFriendToDb(MySqlConnection cn, int clientId, int friendId)
        {
            string Query = "DELETE FROM user_friend WHERE id_user = @idUser AND id_friend = @idFriend";

            MySqlCommand mySqlComm = new MySqlCommand(Query, cn);

            mySqlComm.Parameters.AddWithValue("@idUser", clientId);
            mySqlComm.Parameters.AddWithValue("@idFriend", friendId);

            if (mySqlComm.ExecuteNonQuery() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ManageUserFriend(Client client, Data msgReceived, Data msgToSend)
        {
            string type = msgReceived.strMessage;
            string friendName = msgReceived.strMessage2;

            string Query = "SELECT id_user FROM users WHERE login = @FriendName";

            MySqlCommand mySqlComm = new MySqlCommand(Query, cn);
            mySqlComm.Parameters.AddWithValue("@FriendName", friendName);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = mySqlComm.ExecuteReader();

            int friend_id = 0;

            if (mySqlReader.Read()) //Expecting only one line
            {
                friend_id = mySqlReader.GetInt16(0);

                mySqlReader.Close();

                if (type == "Yes")
                {
                    bool InserClientToDb = AddFriendToDb(cn, client.id, friend_id); //client add friend in db
                    bool InserFriendToDb = AddFriendToDb(cn, friend_id, client.id); //friend add client in db
                                                                                    //respond to client
                    if (InserClientToDb && InserFriendToDb)
                    {
                        OnClientAddFriend(client.strName, friendName);
                        msgToSend.strMessage = "Yes";
                        msgToSend.strMessage2 = friendName;
                        SendServerRespond(ref client, ref msgToSend);
                        msgToSend.strMessage2 = client.strName;
                        msgToSend.strName = friendName;
                        SendMessageToNick(ref client, msgReceived, msgToSend);
                        //SendMessageToSomeone(ref client, msgReceived, msgToSend);
                    }
                    else
                    {
                        msgToSend.strMessage = "No";
                        msgToSend.strMessage2 = friendName;
                    }
                }
                else if (type == "Delete")
                {
                    bool userDeleteFriend = DeleteFriendToDb(cn, client.id, friend_id);
                    bool friendDeleteUser = DeleteFriendToDb(cn, friend_id, client.id);
                    if (userDeleteFriend && friendDeleteUser)
                    {
                        // so user delete friend, friend delete user
                        //need to send to user and friend list of friends
                        msgToSend.strMessage = "Delete";
                        msgToSend.strMessage2 = friendName;
                        SendMessageToSomeone(ref client, msgReceived, msgToSend);
                        //when client get delete then  he will send to server list ask
                    }


                    OnClientDeleteFriend(client.strName, friendName);
                    //todo in client side if receive delete then delete from friend list in user and friend friend_list
                }
                else if (type == "Add")
                {
                    //send to friend thats he want to be friend
                    msgToSend.strMessage2 = client.strName;
                    msgToSend.strName = friendName;
                    SendMessageToNick(ref client, msgReceived, msgToSend);
                }
                else if (type == "No")
                {
                    //friend type no he dont want be as friend
                    msgToSend.strMessage = "No";
                    //msgToSend.strMessage2 = friendName;
                    msgToSend.strMessage2 = client.strName;
                    msgToSend.strName = friendName;
                    SendMessageToNick(ref client, msgReceived, msgToSend);
                }
            }
            else msgToSend.strMessage = "There is no friend that you want to add.";

            cn.Close();
        }

        private void channelDelete(Client client, Data msgReceived, ref Data msgToSend)
        {
            string channelName = msgReceived.strMessage;
            string adminPass = msgReceived.strMessage2;

            string Query = "SELECT admin_password FROM channel WHERE channel_name = @channelName AND id_user_founder = @idUserFounder ;";

            MySqlCommand mySqlComm = new MySqlCommand(Query, cn);
            mySqlComm.Parameters.AddWithValue("@channelName", channelName);
            mySqlComm.Parameters.AddWithValue("@idUserFounder", client.id);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = mySqlComm.ExecuteReader();

            string admPass = "";

            if (mySqlReader.Read()) //Expecting only one line
            {
                admPass = mySqlReader.GetString(0);
                if (adminPass == admPass)
                {
                    //now delete channel from db
                    string delChannelQuery = "DELETE FROM channel c, user_channel uc WHERE c.id_channel = uc.id_channel AND c.channel_name = @channelName AND c.id_user_founder = @idUser";
                    MySqlCommand mySqlDelComm = new MySqlCommand(delChannelQuery, cn);

                    mySqlDelComm.Parameters.AddWithValue("@channelName", channelName);
                    mySqlDelComm.Parameters.AddWithValue("@idUser", client.id);

                    if (mySqlDelComm.ExecuteNonQuery() > 0)
                    {
                        msgToSend.strMessage = "You are deleted your channel: " + channelName;
                        //if channels have channel that deleted
                        foreach (Channel ch in channels)
                        {
                            if (ch.ChannelName == channelName)
                            {
                                channels.Remove(ch);
                            }
                        }

                        client.enterChannels.Remove(channelName);

                        // no needed -> getChannelsFromDB(); //update list of channels
                        //message to others users witch are in channel that has deleted by creator
                    }
                    else msgToSend.strMessage = "You cannot delete your channel by exit with unknown reason (error).";

                    OnClientDeleteChannel(channelName, client.strName);
                }
                else
                    msgToSend.strMessage = "Wrong admin Password for delete Your Channel:" + channelName + "";
            }
            else
                msgToSend.strMessage = "You cannot delete channel that you not own";
            mySqlReader.Close();
            cn.Close();
        }

        private void channelExit(Client client, Data msgReceived, ref Data msgToSend)
        {
            string channelName = msgReceived.strMessage;
            string Query = "SELECT uc.id_channel, c.id_user_founder FROM channel c, user_channel uc WHERE uc.id_channel = c.id_channel AND c.channel_name = @channelName AND uc.id_user = @idUser";

            MySqlCommand mySqlComm = new MySqlCommand(Query, cn);
            mySqlComm.Parameters.AddWithValue("@channelName", channelName);
            mySqlComm.Parameters.AddWithValue("@idUser", client.id);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = mySqlComm.ExecuteReader();

            int idChannelDb = 0;
            int adminId = 0;

            if (mySqlReader.Read())
            {
                idChannelDb = mySqlReader.GetInt16(0);
                adminId = mySqlReader.GetInt16(1);

                mySqlReader.Close();
                if (adminId != client.id)
                {
                    string DelQuery = "DELETE FROM user_channel WHERE id_user = @idUser AND id_channel = @idChannel";
                    MySqlCommand mySqlDelComm = new MySqlCommand(DelQuery, cn);
                    mySqlDelComm.Parameters.AddWithValue("@idUser", client.id);
                    mySqlDelComm.Parameters.AddWithValue("@idChannel", idChannelDb);

                    if (mySqlDelComm.ExecuteNonQuery() > 0)
                    {
                        msgToSend.strMessage = "You are exit from the channel " + channelName + ".";
                        client.enterChannels.Remove(channelName);
                    }
                    else msgToSend.strMessage = "You connot exit: " + channelName + " contact to admin.";

                    OnClientExitChannel(channelName, client.strName);
                }
                else msgToSend.strMessage = "You cannot exit channel that you created";
            }
            else msgToSend.strMessage = "You cannot exit this channel because you not joined";

            msgToSend.strMessage2 = channelName;

            //mySqlReader.Close();
            cn.Close();
        }

        private void channelJoin(Client client, Data msgReceived, ref Data msgToSend, bool afterCreate = false)
        {
            string channelName = msgReceived.strMessage;
            string channelPass = msgReceived.strMessage2;

            string Query = "SELECT id_channel, welcome_Message, enter_password FROM channel WHERE channel_name = @ChannelName;";

            MySqlCommand mySqlComm = new MySqlCommand(Query, cn);
            mySqlComm.Parameters.AddWithValue("@ChannelName", channelName);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = mySqlComm.ExecuteReader();

            int idChannelDb = 0;
            string welcomeMsg = ""; //used for send email notyfication on login to user 
            string enterPassword = "";

            if (mySqlReader.Read()) //Expecting only one line
            {
                idChannelDb = mySqlReader.GetInt16(0);
                welcomeMsg = mySqlReader.GetString(1);
                enterPassword = mySqlReader.GetString(2);

                mySqlReader.Close();

                //msgToSend.strMessage = channelName;

                if (enterPassword == "")
                    msgToSend.strMessage2 = "Send Password";
                else if (channelPass != enterPassword)
                    msgToSend.strMessage2 = "Wrong Password";
                else
                {
                    mySqlComm.CommandText = "INSERT INTO user_channel (id_user, id_channel, join_date) " +
                           "VALUES (@idUser, @idChannel, @joinDate)";

                    DateTime theDate = DateTime.Now;
                    theDate.ToString("dd-MM-yyyy HH:mm");

                    mySqlComm.Parameters.AddWithValue("@idUser", client.id);
                    mySqlComm.Parameters.AddWithValue("@idChannel", idChannelDb);
                    mySqlComm.Parameters.AddWithValue("@joinDate", theDate);

                    if (mySqlComm.ExecuteNonQuery() > 0)
                    {
                        if (!afterCreate)
                        {
                            msgToSend.strMessage2 = "You are joinet to channel " + channelName + ".";
                            msgToSend.strMessage3 = "ChannelJoined";
                        }
                        else
                        {
                            //Send to all users new list of channels
                            cn.Close(); //must be closed because function below open
                                        //getChannelsFromDB();

                            Channel channel = new Channel(channelName, client.id);
                            channels.Add(channel);

                            foreach (Client c in clientList)
                            {
                                sendChannelList(c, ref msgToSend);
                            }

                            SendMessageToAll(ref client, msgToSend); //ignored users wont get new channel list
                                                                     //this below needed for SendServerRespond(ref client, ref msgToSend);
                                                                     ///TODO CHCEK WHY SERVER NOT RESPOND SendServerRespond(ref client, ref msgToSend); AFTER channelJoin
                            msgToSend.strMessage = channelName;
                            msgToSend.strMessage2 = "You are create channel (" + channelName + ")";
                            msgToSend.strMessage3 = "CreatedChannel";
                            msgToSend.cmdCommand = msgReceived.cmdCommand;
                        }
                    }
                    else
                    {
                        msgToSend.strMessage2 = "cannot join to " + channelName + " with unknown reason.";
                    }
                    OnClientJoinChannel(channelName, client.strName);
                }
            }
            else msgToSend.strMessage2 = "There is no channel that you want to join.";

            cn.Close();
        }

        private void channelCreate(Client client, Data msgReceived, ref Data msgToSend)
        {
            string userName = msgReceived.strName;
            string roomName = msgReceived.strMessage;
            string enterPassword = msgReceived.strMessage2;
            string adminPassword = msgReceived.strMessage3;
            string welcomeMsg = msgReceived.strMessage4;

            string Query = "SELECT id_user_founder, channel_name FROM channel WHERE id_user_founder = @id_user_f AND channel_name = @channel_n ;";

            MySqlCommand mySqlComm = new MySqlCommand(Query, cn);
            mySqlComm.Parameters.AddWithValue("@id_user_f", client.id);
            mySqlComm.Parameters.AddWithValue("@channel_n", roomName);
            cn.Open();

            MySqlDataReader mySqlReader = null;
            mySqlReader = mySqlComm.ExecuteReader();

            msgToSend.strMessage2 = "NotCreated";

            int idOfFounderDB = 0;
            string channelNameDB = "";

            if (mySqlReader.Read()) //expecting only one line
            {
                idOfFounderDB = mySqlReader.GetInt16(0);
                channelNameDB = mySqlReader.GetString(1);
            }
            mySqlReader.Close();
            if (channelNameDB == roomName)
                msgToSend.strMessage = "Channel Name is in Use, try other.";
            else if (idOfFounderDB != 0)
                msgToSend.strMessage = "You are create channel before, you can have one channel at time";
            else //user not have channel and name is free
            {
                mySqlComm.CommandText = "INSERT INTO channel (id_user_founder, channel_name, enter_password, admin_password, max_users, create_date, welcome_Message) " +
                    "VALUES (@idUser, @channelName, @enterPass, @adminPass, @maxUsers, @createDate, @welcomeMessage)";

                DateTime theDate = DateTime.Now;
                theDate.ToString("dd-MM-yyyy HH:mm");

                mySqlComm.Parameters.AddWithValue("@idUser", client.id);
                mySqlComm.Parameters.AddWithValue("@channelName", roomName);
                mySqlComm.Parameters.AddWithValue("@enterPass", enterPassword);
                mySqlComm.Parameters.AddWithValue("@adminPass", adminPassword);
                mySqlComm.Parameters.AddWithValue("@maxUsers", 5); ///todo
                mySqlComm.Parameters.AddWithValue("@createDate", theDate);
                mySqlComm.Parameters.AddWithValue("@welcomeMessage", welcomeMsg);

                if (mySqlComm.ExecuteNonQuery() > 0)
                {
                    msgToSend.strMessage = "You are create channel (" + roomName + ")";
                    msgToSend.strMessage2 = "CreatedChannel";
                    cn.Close(); //is must be close, because join have open, makes problem at last line of this fuction
                                //so user create channel, send to him message and join to channel
                                //byte[] message = msgToSend.ToByte();
                                //client.cSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(OnSend), client.cSocket);

                    getChannelsFromDB(); //update list of channels

                    channelJoin(client, msgReceived, ref msgToSend, true); //after user create channel we want to make him join
                }
                else
                    msgToSend.strMessage = "Channel NOT created with unknown reason.";

                OnClientCreateChannel(roomName, client.strName);
            }

            cn.Close();
        }

        private void SendMessageToAll(ref Client conClient, Data msgToSend)
        {
            byte[] message = msgToSend.ToByte();

            foreach (Client cInfo in clientList)
            {
                if (!cInfo.ignoredUsers.Contains(conClient.strName)) //this user witch get ignored by someone, someone will not see when user: login,logout,msg,msg in channels
                    cInfo.cSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(OnSend), cInfo.cSocket);
            }

            OnClientSendMessage(msgToSend.strMessage); //server will not see private messages 
        }
        //send msg to cient and client target
        private void SendMessageToSomeone(ref Client conClient, Data msgReceived, Data msgToSend)
        {
            byte[] message = msgToSend.ToByte();

            foreach (Client cInfo in clientList)
            {
                if (cInfo.strName == msgReceived.strMessage2 || msgReceived.strName == cInfo.strName)
                {
                    cInfo.cSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(OnSend), cInfo.cSocket);
                }
            }
        }

        private void SendMessageToNick(ref Client conClient, Data msgReceived, Data msgToSend)
        {
            byte[] message = msgToSend.ToByte();

            foreach (Client cInfo in clientList)
            {
                if (cInfo.strName == /*msgReceived.strMessage2*/msgToSend.strName)
                {
                    cInfo.cSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(OnSend), cInfo.cSocket);
                }
            }
        }

        private void SendMessageToChannel(ref Client client, string channelName, ref Data msgToSend)
        {
            //first think was i woud serch EVERY user in db that he joined to channel HELL NO i do list of channels in client class 
            byte[] message = msgToSend.ToByte();

            foreach (Client cInfo in clientList)
            {
                if (cInfo.enterChannels.Contains(channelName))
                {
                    cInfo.cSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(OnSend), cInfo.cSocket);
                }
            }
        }

        private void SendServerRespond(ref Client conClient, ref Data msgToSend)
        {
            byte[] message = msgToSend.ToByte();

            conClient.cSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(OnSend), conClient.cSocket);
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
        protected virtual void OnClientList(string cMessage, string cMessage2)
        {
            ClientList?.Invoke(this, new ClientEventArgs() { clientMessageToSend = cMessage, clientMessageTwoToSend = cMessage2 });
        }
        protected virtual void OnClientSendMessage(string cMessage) //brodcasted messages
        {
            ClientSendMessage?.Invoke(this, new ClientEventArgs() { clientMessageToSend = cMessage });// do zrobienia cale data a nie tylko msgMessage
        }
        protected virtual void OnClientReceiMessage(int command, string cName, string cMessage, string cFriendName)
        {
            ClientReceiMessage?.Invoke(this, new ClientEventArgs() { clientCommand = command, clientName = cName, clientMessageReciv = cMessage, clientFriendName = cFriendName });// tu jeszcze nie wiem
        }
        //channel
        protected virtual void OnClientCreateChannel(string channelName, string userName)
        {
            ClientCreateChannel?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
        protected virtual void OnClientJoinChannel(string channelName, string userName)
        {
            ClientJoinChannel?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
        protected virtual void OnClientDeleteChannel(string channelName, string userName)
        {
            ClientDeleteChannel?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
        protected virtual void OnClientExitChannel(string channelName, string userName)
        {
            ClientExitChannel?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
        protected virtual void OnClientEditChannel(string channelName, string userName)
        {
            ClientEditChannel?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
        protected virtual void OnClientChannelMessage(string cMessageToSend, string cMessageRecev)
        {
            ClientChannelMessage?.Invoke(this, new ClientEventArgs() { clientMessageToSend = cMessageToSend, clientMessageReciv = cMessageRecev });
        }
        //friend
        protected virtual void OnClientAddFriend(string ClientName, string ClientFriendName)
        {
            ClientAddFriend?.Invoke(this, new ClientEventArgs() { clientName = ClientName, clientFriendName = ClientFriendName });
        }
        protected virtual void OnClientDeleteFriend(string ClientName, string ClientFriendName)
        {
            ClientDeleteFriend?.Invoke(this, new ClientEventArgs() { clientName = ClientName, clientFriendName = ClientFriendName });
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

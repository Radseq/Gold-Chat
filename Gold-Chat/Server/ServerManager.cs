using CommandClient;
using System;
using System.Collections.Generic;
using System.Data;
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
        public event EventHandler<ClientEventArgs> ClientEditChannel; //edit
        public event EventHandler<ClientEventArgs> ClientChannelMessage;
        //friend
        public event EventHandler<ClientEventArgs> ClientAddFriend;
        public event EventHandler<ClientEventArgs> ClientDeleteFriend;


        private static ManualResetEvent allDone = new ManualResetEvent(false);

        //The collection of all clients logged into the room
        private List<Client> clientList = new List<Client>();
        //list of all channels
        private List<Channel> channels = new List<Channel>();

        ServerLogger servLogger;
        byte[] byteData = new byte[1024];

        //Singleton use db
        DataBaseManager db = DataBaseManager.Instance;

        public ServerManager(ServerLogger servLogg)
        {
            getChannelsFromDB();
            servLogger = servLogg;
        }

        private void getChannelsFromDB()
        {
            string query = "SELECT channel_name, id_user_founder FROM channel";
            DataTable dt = db.manySelect(query);

            foreach (DataRow row in dt.Rows)
            {
                Channel channel = new Channel(row.Field<string>("channel_name"), row.Field<Int64>("id_user_founder"));
                if (!channels.Contains(channel))
                    channels.Add(channel);
            }
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
            clientList.Add(conClient); // When a user logs in to the server then we add her to our list of clients

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
            string newPassword = msgReceived.strMessage;

            db.bind("userName", conClient.strName);
            string oldPassword = db.singleSelect("SELECT password FROM users WHERE login = @userName");

            if (oldPassword != "")
            {
                if (oldPassword != newPassword)
                {
                    msgToSend.strMessage = updateUserPasswordToDb(newPassword, conClient.strName, oldPassword);
                }
                else
                    msgToSend.strMessage = "New and old password are same!";
            }
        }

        private string updateUserPasswordToDb(string newPassword, string userName, string oldPassword)
        {
            db.bind(new string[] { "pass", newPassword, "Login", userName, "oldPass", oldPassword });
            int updated = db.delUpdateInsertDb("UPDATE users SET password = @pass WHERE login = @Login AND password = @oldPass");

            if (updated > 0)
                return "Your Password has been changed!";
            else
                return "Unknow Error while changing password";
        }

        private string CheckUserBan(Int64 id_user)
        {
            db.bind("IdUser", id_user.ToString());
            string endBanDateFromDB = db.singleSelect("SELECT end_ban FROM user_bans WHERE id_user = @IdUser");

            if (endBanDateFromDB != "")
            {
                DateTime dt1 = DateTime.Parse(endBanDateFromDB);
                DateTime dt2 = DateTime.Now;

                if (dt1.Date > dt2.Date)
                    return endBanDateFromDB;
                else
                    return null;
            }
            return null;
        }

        private void clientLogin(ref Client conClient, Data msgReceived, ref Data msgToSend)
        {
            string userName = msgReceived.strName;
            string userPassword = msgReceived.strMessage;
            string loginNotyfiUser = msgReceived.strMessage2;

            db.bind(new string[] { "@userName", userName, "@password", userPassword });
            db.manySelect("SELECT register_id, email, id_user, login, permission FROM users WHERE login = @userName AND password = @password");
            string[] query = db.tableToRow();

            if (query == null || userName != query[3])
                msgToSend.strMessage = "Wrong login or password";
            else if (query[0] != "") // query[0] Activation code must be "" if user want to login
            {
                // User wont send activation code
                msgToSend.cmdCommand = Command.ReSendEmail;
                msgToSend.strMessage = "You must activate your account first.";
            }
            else
            {
                conClient.id = Int64.Parse(query[2]);
                conClient.permission = Int16.Parse(query[4]);

                string ban = CheckUserBan(conClient.id);
                if (ban == null)
                {
                    // All is correct so user can use app
                    conClient.strName = query[3];
                    msgToSend.strMessage = "You are succesfully Log in";
                    msgToSend.strMessage2 = conClient.permission.ToString(); //send to user if he is admin, used to visibility some oprions and rights on client side
                    conClient.enterChannels = new List<string>(); // Init of channels whitch i joined
                    conClient.ignoredUsers = new List<string>(); // Init of ignored users

                    if (loginNotyfiUser == "1") // User wants to be notyficated when login to account
                    {
                        var emailSender = new EmailSender();
                        emailSender.EmailSended += OnEmaiNotyficationLoginSended;
                        emailSender.SendEmail(userName, query[1], "Gold Chat: Login Notyfication", "You have login: " + DateTime.Now.ToString("dd:MM On HH:mm:ss") + " To Gold Chat Account.");
                    }

                    OnClientLogin(userName, conClient.addr.Address.ToString(), conClient.addr.Port.ToString()); // Server OnClientLogin occur only when succes program.cs -> OnClientLogin
                }
                else msgToSend.strMessage = "You are banned untill " + ban;
            }
        }

        private void clientRegistration(Data msgReceived, ref Data msgToSend)
        {
            string userName = msgReceived.strName;
            string userPassword = msgReceived.strMessage;
            string userEmail = msgReceived.strMessage2;

            db.bind(new string[] { "Login", userName, "Email", userEmail });
            db.manySelect("SELECT login, email, register_id FROM users WHERE login = @Login OR email = @Email");
            string[] query = db.tableToRow();

            if (query != null)
            {
                if (query[0] == userName)
                    msgToSend.strMessage = "Your login exists, try other one";
                else if (query[1] == userEmail)
                    msgToSend.strMessage = "Your email exists, try other one";
                else if (query[2] != "")
                    msgToSend.strMessage = "You have already register, on next login you will be ask for register key";
                else
                {
                    insertUserToDb(ref msgToSend, userName, userEmail, userPassword);
                }
            }
            else insertUserToDb(ref msgToSend, userName, userEmail, userPassword);
        }

        private void insertUserToDb(ref Data msgToSend, string userName, string userEmail, string userPassword)
        {
            string registrationCode = CalculateChecksum(userEmail);

            db.bind(new string[] { "user_name", userName, "user_password", userPassword, "user_email", userEmail, "register_id", registrationCode, "perm", 0.ToString() });
            int created = db.delUpdateInsertDb("INSERT INTO users(login, password, email, register_id, permission) " + "VALUES(@user_name, @user_password, @user_email, @register_id, @perm)");

            if (created > 0)
            {
                var emailSender = new EmailSender();
                emailSender.EmailSended += OnEmaiSended;

                emailSender.SendEmail(userName, userEmail, "Gold Chat: Registration", userRegistrationMessage(userName, registrationCode));

                msgToSend.strMessage = "You has been registered";

                OnClientRegistration(userName, userEmail);
            }
            else
                msgToSend.strMessage = "Account NOT created with unknown reason.";
        }

        private void clientReSendActivCode(Data msgReceived, ref Data msgToSend)
        {
            string userName = msgReceived.strName;
            string userRegisterCode = msgReceived.strMessage;
            string regCode = "";
            string userEmail = "";

            if (userRegisterCode != null)
            {
                db.bind(new string[] { "RegId", userRegisterCode, "Login", userName });
                db.manySelect("SELECT register_id, email FROM users WHERE register_id = @RegId AND login = @Login");
                string[] query = db.tableToRow();
                if (query != null)
                {
                    regCode = query[0];
                    userEmail = query[1];

                    if (regCode == userRegisterCode)
                    {
                        db.bind(new string[] { "reg_id", "", "email", userEmail });
                        int updated = db.delUpdateInsertDb("UPDATE users SET register_id = @reg_id WHERE email = @email");

                        if (updated > 0)
                            msgToSend.strMessage = "Now you can login in to application";
                        else
                            msgToSend.strMessage = "Error when Activation contact to support";
                    }
                    else
                        msgToSend.strMessage = "Activation code not match.";
                }
                else msgToSend.strMessage = "No user with that activation code";
            }
            else
            {
                db.bind("login", userName);
                db.manySelect("SELECT register_id, email FROM users WHERE login = @login");
                string[] query = db.tableToRow();
                if (query != null)
                {
                    regCode = query[0];
                    userEmail = query[1];
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
                else msgToSend.strMessage = "You are not registred";
            }
            OnClientReSendAckCode(userName, userEmail);
        }

        // Using as logout and when client crash/internet disconect etc
        // Return name to use when client crashed
        private string clientLogout(ref Client conClient, Data msgToSend, bool isClientCrash = false)
        {
            // When a user wants to log out of the server then we search for him 
            // in the list of clients and close the corresponding connection
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

                foreach (Channel ch in channels) // Dont need to send list etc to user because all channels got msg that user log out and channel check if exists in theirs list
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
        private void sendChannelList(ref Data msgToSend)
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

        // Send to user, list of channels that he joined before
        private void sendChannelListJoined(Client conClient, ref Data msgToSend)
        {
            msgToSend.strName = null;

            db.bind("idUser", conClient.id.ToString());
            db.manySelect("SELECT c.channel_name FROM channel c, user_channel uc WHERE uc.id_channel = c.id_channel AND uc.id_user = @idUser");
            List<string> query = db.tableToColumn();

            msgToSend.strMessage2 = foreachInQuery(query);

            OnClientList(msgToSend.strMessage, msgToSend.strMessage2);
        }
        //todo i send all names form db, what if someone is offline?
        private void sendFriendList(Client conClient, ref Data msgToSend)
        {
            msgToSend.cmdCommand = Command.List;
            msgToSend.strName = null;
            msgToSend.strMessage = "Friends";

            db.bind("idUser", conClient.id.ToString());
            db.manySelect("SELECT u.login FROM users u, user_friend uf WHERE uf.id_friend = u.id_user AND uf.id_user = @idUser");
            List<string> query = db.tableToColumn();

            msgToSend.strMessage2 = foreachInQuery(query);

            OnClientList(msgToSend.strMessage, msgToSend.strMessage2);
        }
        // Using in sendChannelListJoined, sendFriendList
        private string foreachInQuery(List<string> query)
        {
            string returnItems = null;
            foreach (var item in query)
            {
                returnItems += item + "*";
            }
            return returnItems;
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
                        if (msgToSend.strMessage == "You are succesfully Log in") // Client succesfully login and rest of online users will got this msg below
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

                    case Command.lostPassword:
                        clientLostPassword(msgReceived, ref msgToSend);
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

                    case Command.Message: // Text of the message that we will broadcast to all users
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
                        clientCreateChannel(client, msgReceived, ref msgToSend);
                        if (msgToSend.strMessage2 != "CreatedChannel")
                            SendServerRespond(ref client, ref msgToSend);
                        break;

                    case Command.joinChannel:
                        clientJoinChannel(client, msgReceived, ref msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        break;

                    case Command.exitChannel:
                        clientExitChannel(client, msgReceived, ref msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        break;

                    case Command.deleteChannel:
                        clientDeleteChannel(client, msgReceived, ref msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        break;
                    case Command.enterChannel:
                        clientEnterToChannel(client, msgReceived, ref msgToSend);
                        break;
                    case Command.leaveChannel:
                        clientLeaveChannel(ref client, msgReceived.strMessage, msgReceived.strName);
                        SendMessageToChannel(ref client, msgReceived.strMessage, ref msgToSend);
                        break;

                    case Command.List:  // Send the names of all users in the chat room to the new user
                        if (msgReceived.strMessage == "Channel")
                            sendChannelList(ref msgToSend);
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
                    /// Not Implement !!!
                    case Command.kick:
                        ClientKick(client, msgReceived, msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        //msgToSend.strName = friendName;
                        //SendMessageToNick(ref client, msgReceived, msgToSend);
                        break;
                    /// Not Implement !!!
                    case Command.ban:
                        ClientBan(ref client, msgReceived, msgToSend);
                        SendServerRespond(ref client, ref msgToSend);
                        break;
                    /// Not Implement !!!
                    case Command.kickUserChannel:
                        ClientKickFromChannel(ref client, msgReceived, msgToSend);
                        break;
                    /// Not Implement !!!
                    case Command.banUserChannel:
                        ClientBanFromChannel(ref client, msgReceived, msgToSend);
                        break;
                }

                ReceivedMessage(ref client, msgReceived, byteData);
            }
            catch (Exception ex)
            {
                // So we make sure that client which got crash or internet close, server will send log out message
                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Logout;

                string exMessage = ("client: " + clientLogout(ref client, msgToSend, true) + " " + ex.Message);
                Console.WriteLine(exMessage);
                Console.WriteLine(ex.StackTrace);
                servLogger.msgLog(exMessage);

                SendMessageToAll(ref client, msgToSend);

                //if (client is IDisposable) ((IDisposable)client).Dispose(); //free client
            }
        }

        private void clientLostPassword(Data msgReceived, ref Data msgToSend)
        {
            string type = msgReceived.strMessage;

            if (type == "email")
            {
                string emailAdr = msgReceived.strMessage2;

                db.bind("Email", emailAdr);
                db.manySelect("SELECT id_user, email FROM users WHERE email = @Email");
                string[] respond = db.tableToRow();
                if (respond != null)
                {
                    string email = respond[1];
                    string generatedCode = generateRandom(25);
                    if (inserUserLostPasswordCodeToDb(respond, generatedCode) > 0)
                    {
                        sendEmailOnUserLostPassword(email, generatedCode, ref msgToSend);
                    }
                    else msgToSend.strMessage2 = "Unknown error while save random code, contact to admin";
                }
                else msgToSend.strMessage = "That email not exists";
            }
            else if (type == "codeFromEmail")
                clientSendCodeFromEmail(msgReceived, ref msgToSend);
            else msgToSend.strMessage2 = "Wrong operation option";
        }

        private int inserUserLostPasswordCodeToDb(string[] respond, string generatedCode)
        {
            int id_user = Int32.Parse(respond[0]);
            DateTime theDate = DateTime.Now;
            theDate.ToString("dd-MM-yyyy HH:mm");
            db.bind(new string[] { "idUser", id_user.ToString(), "Code", generatedCode, "CodeCreateDate", theDate.ToString() });
            int created = db.delUpdateInsertDb("INSERT INTO user_lost_pass (id_user, code, code_create_date) " + "VALUES (@idUser, @Code, @CodeCreateDate)");
            return created;
        }

        private void sendEmailOnUserLostPassword(string email, string generatedCode, ref Data msgToSend)
        {
            var emailSender = new EmailSender();
            emailSender.EmailSended += OnEmaiSended;

            emailSender.SendEmail("Zgubione Hasło", email, "Gold Chat: Lost Password", userLostPassEmailMessage("Urzytkowniku", generatedCode));

            msgToSend.strMessage2 = "Lost password code has send to your email";
        }

        private void clientSendCodeFromEmail(Data msgReceived, ref Data msgToSend)
        {
            string code = msgReceived.strMessage2;
            string newPassword = msgReceived.strMessage3;

            db.bind("Code", code);
            db.manySelect("SELECT ulp.code, u.email, u.password, u.login FROM user_lost_pass ulp, users u WHERE u.id_user = ulp.id_user AND code = @Code");
            string[] codeDb = db.tableToRow();
            if (codeDb != null && codeDb[0] == code)
            {
                db.bind("Code", code);
                int deleted = db.delUpdateInsertDb("DELETE FROM user_lost_pass WHERE code = @Code");

                if (deleted == 0)
                    Console.WriteLine("Cannot delete " + codeDb[1] + " from user_lost_pass");

                string updated = updateUserPasswordToDb(newPassword, codeDb[3], codeDb[2]);
                msgToSend.strMessage2 = updated;
            }
            else
                msgToSend.strMessage2 = "Wrong code from email";
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
                                        //delete him form channel
                                    }
                            }
                        }
                        else
                            msgToSend.strMessage2 = "There is no " + userName + " in your channel";
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
                        if (insertUserBanToDb(ref client, c, msgToSend, banReason, time) == 0) msgToSend.strMessage2 = "Cannot ban " + userName + " unknown reason";
                    }
                }
            }
            else msgToSend.strMessage = "You dont have permission to kick " + userName;
        }

        private int insertUserBanToDb(ref Client client, Client c, Data msgToSend, string banReason, string time)
        {
            db.bind(new string[] { "idUser", c.id.ToString(), "BanReason", banReason, "EndBanDateTime", time });
            int created = db.delUpdateInsertDb("INSERT INTO user_bans (id_user, reason, end_ban) " + "VALUES (@idUser, @BanReason, @EndBanDateTime)");

            if (created > 0)
            {
                SendMessageToAll(ref client, msgToSend);
                c.cSocket.Close();
                return 1;
            }
            return 0;
        }

        private void ClientKick(Client client, Data msgReceived, Data msgToSend)
        {
            string userName = msgReceived.strMessage;       // Nick of kicked user
            string kickReason = msgReceived.strMessage2;    // Reason of kick

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

            db.bind("IgnoreName", userName);
            db.manySelect("SELECT id_user FROM users WHERE login = @IgnoreName");
            List<string> users = db.tableToColumn();

            if (users.Capacity > 1)
            {
                msgToSend.strMessage3 = userName;

                if (type == "AddIgnore")
                {
                    if (!client.ignoredUsers.Contains(userName))
                    {
                        msgToSend.strMessage2 = addIgnoredUserToDb(client, userName, users);
                    }
                    else
                        msgToSend.strMessage2 = "Cannot ignore " + userName + " because already ignored!";
                }
                else if (type == "DeleteIgnore")
                {
                    if (client.ignoredUsers.Contains(userName))
                    {
                        msgToSend.strMessage2 = deleteIgnoredUserFromDb(client, userName, users);
                    }
                    else
                        msgToSend.strMessage2 = "Cannot delete ignore from " + userName + " because not ignored!";
                }
                else msgToSend.strMessage2 = "There is only Add or Delete users option";
            }
            else msgToSend.strMessage2 = "Contact to admin because is too many users with nick" + userName;
        }

        private string addIgnoredUserToDb(Client client, string userName, List<string> users)
        {
            db.bind(new string[] { "idUser", client.id.ToString(), "idUserIgnored", users[0] });
            int created = db.delUpdateInsertDb("INSERT INTO user_ignored (id_user, id_user_ignored) " + "VALUES (@idUser, @idUserIgnored)");

            if (created > 0)
            {
                client.ignoredUsers.Add(userName);
                return "You are now ignore: " + userName;
                //OnClientIgnoreUser(client.strName, friendName);
            }
            else
                return "Cannot ignore " + userName + " unknown reason";
        }

        private string deleteIgnoredUserFromDb(Client client, string userName, List<string> users)
        {
            db.bind(new string[] { "idUser", client.id.ToString(), "idUserIgnored", users[0] });
            int deleted = db.delUpdateInsertDb("DELETE FROM user_ignored WHERE id_user = @idUser AND id_user_ignored = @idUserIgnored");

            if (deleted > 0)
            {
                client.ignoredUsers.Remove(userName);
                return "You are delete from ignore list user: " + userName;
            }
            else
                return "Cannot delete ignore from " + userName + " unknown reason";
        }

        private void sendListIgnoredUsers(Client client, ref Data msgToSend, ref Data msgReceived)
        {
            msgToSend.cmdCommand = Command.List;
            msgToSend.strName = null;
            msgToSend.strMessage = "IgnoredUsers";

            db.bind("idUser", client.id.ToString());
            db.manySelect("SELECT u.login FROM users u, user_ignored ui WHERE ui.id_user_ignored = u.id_user AND ui.id_user = @idUser");
            List<string> ignoredUsers = db.tableToColumn();
            foreach (var item in ignoredUsers)
            {
                msgToSend.strMessage2 += item + "*";
                client.ignoredUsers.Add(item);
            }

            OnClientList(msgToSend.strMessage, msgToSend.strMessage2);
        }

        // When user enter to channel, he send to server ask for users already in this channel
        private void sendListUsersInChannel(Client client, ref Data msgToSend, ref Data msgReceived)
        {
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

        private void clientLeaveChannel(ref Client client, string channelName, string userName)
        {
            // Name of user and name of channel send to users in channels
            client.enterChannels.Remove(channelName);
            // Remove user form channel users
            foreach (Channel ch in channels)
            {
                if (ch.ChannelName == channelName)
                {
                    ch.Users.Remove(userName);
                }
            }
            //OnClientLeaveChannel(channelName, client.strName); //todo
        }

        private void clientEnterToChannel(Client client, Data msgReceived, ref Data msgToSend)
        {
            string channelName = msgReceived.strMessage;

            db.bind(new string[] { "channelName", channelName, "idUser", client.id.ToString() });
            db.manySelect("SELECT uc.id_channel, c.welcome_message FROM channel c, user_channel uc WHERE uc.id_channel = c.id_channel AND c.channel_name = @channelName AND uc.id_user = @idUser");

            string[] respond = db.tableToRow();
            if (respond != null)
            {
                int id_channel_db = Int32.Parse(respond[0]);
                string motd = respond[1];

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
                msgToSend.strMessage3 = "Cannot Enter Because you not join to " + channelName;
                SendServerRespond(ref client, ref msgToSend);
            }
        }

        // Used in ManageUserFriend function
        private bool AddFriendToDb(Int64 clientId, Int64 friendId)
        {
            db.bind(new string[] { "idUser", clientId.ToString(), "idFriend", friendId.ToString() });
            int created = db.delUpdateInsertDb("INSERT INTO user_friend (id_user, id_friend) " + "VALUES (@idUser, @idFriend)");

            if (created > 0)
                return true;
            else
                return false;

        }
        // Used in ManageUserFriend function
        private bool DeleteFriendToDb(Int64 clientId, Int64 friendId)
        {
            db.bind(new string[] { "idUser", clientId.ToString(), "idFriend", friendId.ToString() });
            int deleted = db.delUpdateInsertDb("DELETE FROM user_friend WHERE id_user = @idUser AND id_friend = @idFriend");

            if (deleted > 0)
                return true;
            else
                return false;
        }

        private void ManageUserFriend(Client client, Data msgReceived, Data msgToSend)
        {
            string type = msgReceived.strMessage;
            string friendName = msgReceived.strMessage2;

            db.bind("FriendName", friendName);
            Int64 friend_id = Int64.Parse(db.singleSelect("SELECT id_user FROM users WHERE login = @FriendName"));

            if (friend_id > 0)
            {
                if (type == "Yes")
                {
                    bool InserClientToDb = AddFriendToDb(client.id, friend_id); // client add friend in db
                    bool InserFriendToDb = AddFriendToDb(friend_id, client.id); // friend add client in db

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
                    bool userDeleteFriend = DeleteFriendToDb(client.id, friend_id);
                    bool friendDeleteUser = DeleteFriendToDb(friend_id, client.id);
                    if (userDeleteFriend && friendDeleteUser)
                    {
                        // So user delete friend, friend delete user
                        // Need to send to user and friend list of friends
                        msgToSend.strMessage = "Delete";
                        msgToSend.strMessage2 = friendName;
                        SendMessageToSomeone(ref client, msgReceived, msgToSend);
                        //when client get delete then  he will send to server list ask
                        OnClientDeleteFriend(client.strName, friendName);
                    }
                }
                else if (type == "Add")
                {
                    // Send to friend thats he want to be friend
                    msgToSend.strMessage2 = client.strName;
                    msgToSend.strName = friendName;
                    SendMessageToNick(ref client, msgReceived, msgToSend);
                }
                else if (type == "No")
                {
                    // Friend type no: he dont want be as friend
                    msgToSend.strMessage = "No";
                    msgToSend.strMessage2 = client.strName;
                    msgToSend.strName = friendName;
                    SendMessageToNick(ref client, msgReceived, msgToSend);
                }
            }
            else msgToSend.strMessage = "There is no friend that you want to add.";
        }

        private void clientDeleteChannel(Client client, Data msgReceived, ref Data msgToSend)
        {
            string channelName = msgReceived.strMessage;
            string adminPass = msgReceived.strMessage2;

            db.bind(new string[] { "channelName", channelName, "idUserFounder", client.id.ToString() });

            string admPass = db.singleSelect("SELECT admin_password FROM channel WHERE channel_name = @channelName AND id_user_founder = @idUserFounder");
            if (adminPass != "")
            {
                if (adminPass == admPass)
                    msgToSend.strMessage = deleteChannelFromDb(client, channelName);// Delete channel from db
                else
                    msgToSend.strMessage = "Wrong admin Password for delete Your Channel:" + channelName + "";
            }
            else
                msgToSend.strMessage = "You cannot delete channel that you not own";
        }

        private string deleteChannelFromDb(Client client, string channelName)
        {
            db.bind(new string[] { "channelName", channelName, "idUser", client.id.ToString() });
            int deleted = db.delUpdateInsertDb("DELETE FROM channel c, user_channel uc WHERE c.id_channel = uc.id_channel AND c.channel_name = @channelName AND c.id_user_founder = @idUser");

            if (deleted > 0)
            {
                // If channels list have channel, delete this channel from list
                foreach (Channel ch in channels)
                {
                    if (ch.ChannelName == channelName)
                    {
                        channels.Remove(ch);
                    }
                }
                client.enterChannels.Remove(channelName);
                OnClientDeleteChannel(channelName, client.strName);
                return "You are deleted your channel: " + channelName;

                //TODO message to others users witch are in channel that has deleted by creator
            }
            else return "You cannot delete your channel by exit with unknown reason (error).";
        }

        private void clientExitChannel(Client client, Data msgReceived, ref Data msgToSend)
        {
            string channelName = msgReceived.strMessage;

            db.bind(new string[] { "channelName", channelName, "idUser", client.id.ToString() });
            db.manySelect("SELECT uc.id_channel, c.id_user_founder FROM channel c, user_channel uc WHERE uc.id_channel = c.id_channel AND c.channel_name = @channelName AND uc.id_user = @idUser");
            string[] getFromDb = db.tableToRow();
            if (getFromDb != null)
            {
                int idChannelDb = Int32.Parse(getFromDb[0]);
                Int64 adminId = Int64.Parse(getFromDb[1]);

                if (idChannelDb > 0 && adminId > 0)
                {
                    if (adminId != client.id)
                    {
                        msgToSend.strMessage = deleteUserFromChannelDb(client, channelName, idChannelDb);
                    }
                    else msgToSend.strMessage = "You cannot exit channel that you created";
                }
                else msgToSend.strMessage = "You cannot exit this channel because you not joined";
            }
            else msgToSend.strMessage = "Channel not exit or you are not joined to";

            msgToSend.strMessage2 = channelName;
        }

        private string deleteUserFromChannelDb(Client client, string channelName, int idChannelDb)
        {
            db.bind(new string[] { "idUser", client.id.ToString(), "idChannel", idChannelDb.ToString() });
            int deleted = db.delUpdateInsertDb("DELETE FROM user_channel WHERE id_user = @idUser AND id_channel = @idChannel");

            if (deleted > 0)
            {
                client.enterChannels.Remove(channelName);

                OnClientExitChannel(channelName, client.strName);

                return "You are exit from the channel " + channelName + ".";
            }
            else return "You connot exit: " + channelName + " contact to admin.";
        }

        private void clientJoinChannel(Client client, Data msgReceived, ref Data msgToSend, bool afterCreate = false)
        {
            string channelName = msgReceived.strMessage;
            string channelPass = msgReceived.strMessage2;

            db.bind("ChannelName", channelName);
            db.manySelect("SELECT id_channel, welcome_Message, enter_password FROM channel WHERE channel_name = @ChannelName");
            string[] getFromDb = db.tableToRow();
            if (getFromDb != null)
            {
                int idChannelDb = Int32.Parse(getFromDb[0]);
                string welcomeMsg = getFromDb[1]; // Used for send email notyfication when user login 
                string enterPassword = getFromDb[2];

                if (enterPassword == null)
                    msgToSend.strMessage2 = "Send Password";
                else if (channelPass != enterPassword)
                    msgToSend.strMessage2 = "Wrong Password";
                else
                    insertUserJoinedChannelToDb(client, msgReceived, ref msgToSend, idChannelDb, channelName, afterCreate);
            }
            else msgToSend.strMessage2 = "There is no channel that you want to join.";
        }

        private void insertUserJoinedChannelToDb(Client client, Data msgReceived, ref Data msgToSend, int idChannelDb, string channelName, bool afterCreate)
        {
            DateTime theDate = DateTime.Now;
            theDate.ToString("dd-MM-yyyy HH:mm");

            db.bind(new string[] { "idUser", client.id.ToString(), "idChannel", idChannelDb.ToString(), "joinDate", theDate.ToString() });
            int created = db.delUpdateInsertDb("INSERT INTO user_channel (id_user, id_channel, join_date) " + "VALUES (@idUser, @idChannel, @joinDate)");

            if (created > 0)
            {
                if (!afterCreate)
                {
                    msgToSend.strMessage2 = "You are joinet to channel " + channelName + ".";
                    msgToSend.strMessage3 = "ChannelJoined";
                }
                else
                {
                    sendChannelList(ref msgToSend);
                    SendMessageToAll(ref client, msgToSend); // Ignored users wont get new channel list

                    msgToSend.cmdCommand = msgReceived.cmdCommand;
                    msgToSend.strMessage = "You are create channel (" + channelName + ")";
                    msgToSend.strMessage2 = "CreatedChannel";
                }
                OnClientJoinChannel(channelName, client.strName);
            }
            else
                msgToSend.strMessage2 = "cannot join to " + channelName + " with unknown reason.";
        }

        private void clientCreateChannel(Client client, Data msgReceived, ref Data msgToSend)
        {
            string roomName = msgReceived.strMessage;

            db.bind(new string[] { "id_user_f", client.id.ToString(), "channel_n", roomName });
            db.manySelect("SELECT id_user_founder, channel_name FROM channel WHERE id_user_founder = @id_user_f AND channel_name = @channel_n");
            string[] getFromDb = db.tableToRow();

            msgToSend.strMessage2 = "NotCreated";

            if (getFromDb != null)
            {
                int idOfFounderDB = Int32.Parse(getFromDb[0]);
                string channelNameDB = getFromDb[1];

                if (channelNameDB == roomName)
                    msgToSend.strMessage = "Channel Name is in Use, try other.";
                else if (idOfFounderDB != 0)
                    msgToSend.strMessage = "You are create channel before, you can have one channel at time";
                else // User not have channel and name is free
                {
                    insertChannelToDb(client, msgReceived, ref msgToSend);
                }
            }
            else insertChannelToDb(client, msgReceived, ref msgToSend); // There is no exists channelName and idfounder, so we can create channel
        }

        private void insertChannelToDb(Client client, Data msgReceived, ref Data msgToSend)
        {
            string userName = msgReceived.strName;
            string roomName = msgReceived.strMessage;
            string enterPassword = msgReceived.strMessage2;
            string adminPassword = msgReceived.strMessage3;
            string welcomeMsg = msgReceived.strMessage4;

            DateTime theDate = DateTime.Now;
            theDate.ToString("dd-MM-yyyy HH:mm");

            db.bind(new string[] { "idUser", client.id.ToString(), "channelName", roomName, "enterPass", enterPassword, "adminPass", adminPassword, "maxUsers", 5.ToString(), "createDate", theDate.ToString(), "welcomeMessage", welcomeMsg });
            int created = db.delUpdateInsertDb("INSERT INTO channel (id_user_founder, channel_name, enter_password, admin_password, max_users, create_date, welcome_Message) " +
                "VALUES (@idUser, @channelName, @enterPass, @adminPass, @maxUsers, @createDate, @welcomeMessage)");

            if (created > 0)
            {
                msgToSend.strMessage = "You are create channel (" + roomName + ")";
                msgToSend.strMessage2 = "CreatedChannel";

                // Add channel to as channels list
                Channel channel = new Channel(roomName, client.id);
                channels.Add(channel);

                clientJoinChannel(client, msgReceived, ref msgToSend, true); // After user create channel we want to make him join
            }
            else
                msgToSend.strMessage = "Channel NOT created with unknown reason.";

            OnClientCreateChannel(roomName, client.strName);
        }

        private void SendMessageToAll(ref Client conClient, Data msgToSend)
        {
            byte[] message = msgToSend.ToByte();

            foreach (Client cInfo in clientList)
            {
                if (!cInfo.ignoredUsers.Contains(conClient.strName)) // This user(foreach) which get ignored by someone, someone will not see when user: login,logout,msg,msg in channels
                    cInfo.cSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(OnSend), cInfo.cSocket);
            }

            OnClientSendMessage(msgToSend.strMessage); // Server will not see private messages 
        }
        // Send msg to cient and client target
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
                conClient.cSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), conClient);
            }
            else if (msgReceived.strMessage != null) // I want to see messages, messages will be null on login/logout
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

        private string userRegistrationMessage(string userName, string registrationCode)
        {
            return string.Format(@"
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
        }

        private string userLostPassEmailMessage(string userName, string code)
        {
            return string.Format(@"
            <p>Witaj <strong>{0}</strong>.
            </p>
            <p><br />
                Jeśli zapomniałeś haśło wklej ten kod w oknie Lost Password : <span style='color: #ff0000;'><strong>{1}</strong></span>
            </p>
            <p>
            <p> 
                Jeśli nie zapomniałeś hasło lub nie próbowałeś go odzyskiwać, usuń tego maila
            </p>
                <strong>
                <span style='color: #ff0000;'> Pamiętaj by dokłanie skopiować KOD.</span>
                </strong>
            </p>
            <p>Dziękujemy <br /> Administracja Gold Chat.</p>", userName, code);
        }

        private string generateRandom(int lenght)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var stringChars = new char[lenght];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }
    }
}

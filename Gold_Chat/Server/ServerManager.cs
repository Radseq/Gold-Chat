using CommandClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

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
        public int clientCommand { get; set; }
        public string clientFriendName { get; set; }

        //or just give this object public ServerManager ServerManager { get; set; }
    }

    //This class represents a client connected to server
    public class ServerManager
    {
        public event EventHandler<ClientEventArgs> ClientLogin;
        public event EventHandler<ClientEventArgs> ClientLogout;
        public event EventHandler<ClientEventArgs> ClientMessage;
        public event EventHandler<ClientEventArgs> ClientList;
        public event EventHandler<ClientEventArgs> ClientSendMessage;
        public event EventHandler<ClientEventArgs> ClientReceiMessage;

        byte[] message;
        //private static byte[] byteData = new byte[1024];

        //Transform the array of bytes received from the user into an
        //intelligent form of object Data

        //Data msgReceived = new Data(byteData);

        //We will send this object in response the users request
        //Data msgToSend = new Data();

        Client clientInfo;

        public void clientLogin(ref Client client, Data msgReceived, Data msgToSend)
        {
            clientInfo = client;
            //client.strName = msgReceived.strName;
            //server_log.AppendText(clientSocket.RemoteEndPoint.ToString());

            string clientIpAdress = ((IPEndPoint)clientInfo.cSocket.RemoteEndPoint).Address.ToString();
            string clientPort = ((IPEndPoint)clientInfo.cSocket.RemoteEndPoint).Port.ToString();
            msgToSend.strMessage = "<<<" + clientInfo.strName + " has joined the room>>>";

            OnClientLogin(clientInfo.strName, clientIpAdress, clientPort);
        }

        //using as logout and when client crash/internet disconect etc
        //return name to use with client crashed
        public string clientLogout(ref List<Client> clientList, ref Socket soc, /*Data msgReceived,*/ Data msgToSend, bool isClientCrash = false)
        {
            int nIndex = 0;
            foreach (Client client in clientList)
            {
                if (client.cSocket == soc)
                {
                    clientList.RemoveAt(nIndex);
                    if (isClientCrash) msgToSend.strName = clientInfo.strName;
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

        public void clientList(List<Client> clientList, Data msgToSend)
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

        public void SendMessage(List<Client> clientList, Data msgReceived, Data msgToSend, bool isPrivateMessage = false)
        {
            message = msgToSend.ToByte();

            foreach (Client cInfo in clientList)
            {
                if (isPrivateMessage ? cInfo.strName == msgReceived.friendName : cInfo.cSocket != clientInfo.cSocket || msgToSend.cmdCommand != Command.Login)
                {
                    //Send the message to all users, or to friend
                    cInfo.cSocket.Send(message, 0, message.Length, SocketFlags.None);
                }
            }
            if (isPrivateMessage == false)
                OnClientSendMessage(msgToSend);
        }

        public void ReceivedMessage(Data msgReceived, byte[] byteData)
        {
            if (msgReceived.cmdCommand != Command.Logout)
            {
                //Start listening to the message send by the user
                clientInfo.cSocket.Receive(byteData, byteData.Length, SocketFlags.None);
                //KeepProcessing = false;
            }
            else if (msgReceived.strMessage != null)// i want to see messages messages will be null on login/logout
            {
                OnClientReceiMessage((int)msgReceived.cmdCommand, msgReceived.strName, msgReceived.strMessage, msgReceived.friendName);
            }
        }

        protected virtual void OnClientLogin(string cName, string cIpadress, string cPort)
        {
            ClientLogin?.Invoke(this, new ClientEventArgs() { clientName = cName, clientIpAdress = cIpadress, clientPort = cPort });
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
        protected virtual void OnClientSendMessage(Data cMessage) //brodcasted messages
        {
            ClientSendMessage?.Invoke(this, new ClientEventArgs() { clientMessageToSend = cMessage.strMessage });// do zrobienia cale data a nie tylko msgMessage
        }
        protected virtual void OnClientReceiMessage(int command, string cName, string cMessage, string cFriendName)
        {
            ClientReceiMessage?.Invoke(this, new ClientEventArgs() { clientCommand = command, clientName = cName, clientMessageReciv = cMessage, clientFriendName = cFriendName });// tu jeszcze nie wiem
        }
    }
}

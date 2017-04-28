﻿using System;
using System.IO;

namespace Server
{
    public class ServerLogger
    {
        // Singleton
        static ServerLogger instance = null;
        static readonly object padlock = new object();

        static string dateFile = DateTime.Now.ToString("dd_MM_yyyy");
        static StreamWriter strWriter = new StreamWriter("ServerLogger-" + dateFile + ".txt", true);

        // Singleton
        public static ServerLogger Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new ServerLogger();

                    return instance;
                }
            }
        }

        private ServerLogger()
        {
            strWriter.WriteLine("Serwer started at " + DateTime.Now.ToString("dd:MM On HH:mm:ss") + " Start Logging.");
            strWriter.Flush();
        }

        private void write(string message)
        {
            strWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + message);
            strWriter.Flush();
        }

        public void OnClientReceiMessageLogger(object sender, ClientEventArgs e)
        {
            write("ReceivedMessage " + e.clientMessageReciv);
        }

        public void OnClientSendMessageLogger(object sender, ClientEventArgs e)
        {
            write("SendMessage " + e.clientMessageToSend);
        }

        /*public void OnClientListLogger(object sender, ClientEventArgs e)
        {
            sw.WriteLine(e.clientMessageToSend);
        }*/

        public void OnClientMessageLogger(object sender, ClientEventArgs e)
        {
            write("clientMessage " + e.clientMessageReciv);
        }

        public void OnClientLogoutLogger(object sender, ClientEventArgs e)
        {
            Console.WriteLine(e.clientName + " has left the room>>>");
            write(e.clientName + " has left the room>>>");
        }

        public void OnClientReSendAckCode(object sender, ClientEventArgs e)
        {
            Console.WriteLine(e.clientName + " resend thier activation code to " + e.clientEmail);
            write(e.clientName + " resend thier activation code to " + e.clientEmail);
        }

        public void OnClientLogin(object sender, ClientEventArgs e)
        {
            Console.WriteLine(e.clientName + " has joined the room>>>");
            write(e.clientName + " has joined the room>>>");
        }

        public void OnClientRegistration(object sender, ClientEventArgs e)
        {
            write(e.clientName + " has registered by " + e.clientEmail);
        }

        public void RunServerLogger(Exception ex)
        {
            write("Exception occured when running: " + ex);
        }

        public void msgLog(string msg)
        {
            write(msg);
        }

        public void OnExecuteReader(object sender, DataBaseManagerEventArgs e)
        {
            string exception = "Exception ExecuteReader : " + e.Exception + "\n\r SQL Query : \n\r" + e.Query;
            Console.WriteLine(exception);
            write(exception);
        }

        public void OnExecuteNonQuery(object sender, DataBaseManagerEventArgs e)
        {
            string exception = "Exception ExecuteNonQuery : " + e.Exception + "\n\r SQL Query : \n\r" + e.Query;
            Console.WriteLine(exception);
            write(exception);
        }

        public void OnConnectToDB(object sender, DataBaseManagerEventArgs e)
        {
            string exception = "Exception Connection : " + e.Exception + "\n\r Closing Application. \n\r";
            Console.WriteLine(exception);
            write(exception);
            Environment.Exit(1);
        }

        public void OnEmaiSended(object source, EmailSenderEventArgs args)
        {
            string outStr = "Activation Code has been send to " + args.UserNameEmail + " email";
            Console.WriteLine(outStr);
            msgLog(outStr);
        }

        public void OnEmaiReSended(object source, EmailSenderEventArgs args)
        {
            string outStr = "Register Code resended to " + args.UserNameEmail + " email";
            Console.WriteLine(outStr);
            msgLog(outStr);
        }

        public void OnEmaiNotyficationLoginSended(object sender, EmailSenderEventArgs e)
        {
            string outStr = "Login Notyfication to " + e.UserNameEmail + " email";
            Console.WriteLine(outStr);
            msgLog(outStr);
        }

    }
}

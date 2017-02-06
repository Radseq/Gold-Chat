﻿using System;
using System.IO;

namespace Server
{
    public class ServerLogger
    {
        //private string file = "serverLogger.txt";
        private StreamWriter sw;

        public ServerLogger(ref StreamWriter strWriter)
        {
            sw = strWriter;
            sw.WriteLine("Serwer started at " + DateTime.Now.ToString("dd:MM On HH:mm:ss") + " Start Logging.");
            sw.Flush();
        }

        private void write(string message)
        {
            sw.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + message);
            sw.Flush();
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
            write(e.clientName + " has left the room>>>");
        }

        public void OnClientLoginLogger(object sender, ClientEventArgs e)
        {
            write(e.clientName + " has joined the room>>>");
        }

        public void RunServerLogger(Exception ex)
        {
            write("Exception occured when running: " + ex);
        }

        public void msgLog(string msg)
        {
            write(msg);
        }
    }
}
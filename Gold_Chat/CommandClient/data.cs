using System;
using System.Collections.Generic;
using System.Text;

namespace CommandClient
{
    //The commands for interaction between the server and the client
    public enum Command
    {
        Login,          //Log into the server
        Logout,         //Logout of the server
        Message,        //Send a text message to all the chat clients
        List,           //Get a list of users in the chat room from the server
        privMessage,    //Support private message from friend

        Reg,            //Registration 
        ReSendEmail,    //Resend activation name to client

        changePassword,//when client want to change password
        //loginNotyfi,//when client want to be notificated on login

        Null            //No command
    }

    //The data structure by which the server and the client interact with 
    //each other
    public class Data
    {
        public string strName;      //Name by which the client logs into the room
        public string strMessage;   //Name of friend.
        public string strMessage2;   //Message text
        public Command cmdCommand;  //Command type (login, logout, send message, etcetera)

        //Default constructor
        public Data()
        {
            cmdCommand = Command.Null;
            strMessage2 = null;
            strName = null;
            strMessage = null;
        }

        //Converts the bytes into an object of type Data
        public Data(byte[] data)
        {
            //The first four bytes are for the Command
            cmdCommand = (Command)BitConverter.ToInt32(data, 0);

            //The next four store the length of the name
            int nameLen = BitConverter.ToInt32(data, 4);

            //name of friend
            int strMessageLen = BitConverter.ToInt32(data, 8);

            //The next four store the length of the message
            int strMessage2Len = BitConverter.ToInt32(data, 12);

            //This check makes sure that strName has been passed in the array of bytes
            if (nameLen > 0)
                strName = Encoding.UTF8.GetString(data, 16, nameLen);
            else
                strName = null;


            //This check makes sure that strName has been passed in the array of bytes
            if (strMessageLen > 0)
                strMessage = Encoding.UTF8.GetString(data, 16 + nameLen, strMessageLen);//tu byl blad odczytywalo 1bit nazwy urzytkownika zamista frienda bo friend name zaczynl sie od x bitu nazwy uzytkownika ....
            else
                strMessage = null;

            //This checks for a null message field
            if (strMessage2Len > 0)
                strMessage2 = Encoding.UTF8.GetString(data, 16 + nameLen + strMessageLen, strMessage2Len);
            else
                strMessage2 = null;
        }

        //Converts the Data structure into an array of bytes
        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();

            //First four are for the Command
            result.AddRange(BitConverter.GetBytes((int)cmdCommand));

            //Add the length of the name
            if (strName != null)
                result.AddRange(BitConverter.GetBytes(strName.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Add the length of the friend name
            if (strMessage != null)
                result.AddRange(BitConverter.GetBytes(strMessage.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Length of the message
            if (strMessage2 != null)
                result.AddRange(BitConverter.GetBytes(strMessage2.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Add the name
            if (strName != null)
                result.AddRange(Encoding.UTF8.GetBytes(strName));

            //Add the friend name
            if (strMessage != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage));

            //And, lastly we add the message text to our array of bytes
            if (strMessage2 != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage2));

            return result.ToArray();
        }
    }
}

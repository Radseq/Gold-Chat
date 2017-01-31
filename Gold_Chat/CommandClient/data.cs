using System;
using System.Collections.Generic;
using System.Text;

namespace CommandClient
{
    //The commands for interaction between the server and the client
    public enum Command
    {
        Login,      //Log into the server
        Logout,     //Logout of the server
        Message,    //Send a text message to all the chat clients
        List,       //Get a list of users in the chat room from the server
        privMessage,//Support private message from friend
        Life,       //checks if user is still connected not using
        Null        //No command
    }

    //The data structure by which the server and the client interact with 
    //each other
    public class Data
    {
        public string strName;      //Name by which the client logs into the room
        public string friendName;   //Name of friend.
        public string strMessage;   //Message text
        public Command cmdCommand;  //Command type (login, logout, send message, etcetera)

        //Default constructor
        public Data()
        {
            cmdCommand = Command.Null;
            strMessage = null;
            strName = null;
            friendName = null;
        }

        //Converts the bytes into an object of type Data
        public Data(byte[] data)
        {
            //The first four bytes are for the Command
            cmdCommand = (Command)BitConverter.ToInt32(data, 0);

            //The next four store the length of the name
            int nameLen = BitConverter.ToInt32(data, 4);

            //name of friend
            int friendNameLen = BitConverter.ToInt32(data, 8);

            //The next four store the length of the message
            int msgLen = BitConverter.ToInt32(data, 12);

            //This check makes sure that strName has been passed in the array of bytes
            if (nameLen > 0)
                strName = Encoding.UTF8.GetString(data, 16, nameLen);
            else
                strName = null;


            //This check makes sure that strName has been passed in the array of bytes
            if (friendNameLen > 0)
                friendName = Encoding.UTF8.GetString(data, 16 + nameLen, friendNameLen);//tu byl blad odczytywalo 1bit nazwy urzytkownika zamista frienda bo friend name zaczynl sie od x bitu nazwy uzytkownika ....
            else
                friendName = null;

            //This checks for a null message field
            if (msgLen > 0)
                strMessage = Encoding.UTF8.GetString(data, 16 + nameLen + friendNameLen, msgLen);
            else
                strMessage = null;
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
            if (friendName != null)
                result.AddRange(BitConverter.GetBytes(friendName.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Length of the message
            if (strMessage != null)
                result.AddRange(BitConverter.GetBytes(strMessage.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Add the name
            if (strName != null)
                result.AddRange(Encoding.UTF8.GetBytes(strName));

            //Add the friend name
            if (friendName != null)
                result.AddRange(Encoding.UTF8.GetBytes(friendName));

            //And, lastly we add the message text to our array of bytes
            if (strMessage != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage));

            return result.ToArray();
        }
    }
}

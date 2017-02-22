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
        List,           //Get a list of users/channels/joinedChannels/friends
        privMessage,    //Support private message from friend

        Reg,            //Registration 
        ReSendEmail,    //Resend activation name to client

        changePassword, //when client want to change password

        createChannel,  //user want to create channel
        joinChannel,    //Join to channel by new user need password
        exitChannel,    //if user want definetly exit channel, he cant enter again untill use join again
        deleteChannel,  //if owner want to delete
        editChannel,    //if owner want to edit 
        leaveChannel,   //inform other users that someone is leave from channel, user that join before
        enterChannel,   //inform other users that someone is enter to channel, user that join before

        manageFriend,   //delete/add/accept_ask friend

        Null            //No command
    }

    //The data structure by which the server and the client interact with 
    //each other
    public class Data
    {
        public string strName;      //Name by which the client logs into the room
        public string strMessage;   //MessageOne
        public string strMessage2;  //MessageTwo
        public string strMessage3;  //MessageThree
        public string strMessage4;  //MessageFour
        public Command cmdCommand;  //Command type (login, logout, send message, etcetera)

        //Default constructor
        public Data()
        {
            cmdCommand = Command.Null;
            strName = null;
            strMessage = null;
            strMessage2 = null;
            strMessage3 = null;
            strMessage4 = null;
        }

        //Converts the bytes into an object of type Data
        public Data(byte[] data)
        {
            //The first four bytes are for the Command
            cmdCommand = (Command)BitConverter.ToInt32(data, 0);

            //The next four store the length of the name
            int nameLen = BitConverter.ToInt32(data, 4);

            //The next four store the length of the strMessage
            int strMessageLen = BitConverter.ToInt32(data, 8);

            //The next four store the length of the strMessage2
            int strMessage2Len = BitConverter.ToInt32(data, 12);

            //The next four store the length of the strMessage3
            int strMessage3Len = BitConverter.ToInt32(data, 16);

            //The next four store the length of the strMessage4
            int strMessage4Len = BitConverter.ToInt32(data, 20);

            if (nameLen > 0)
                strName = Encoding.UTF8.GetString(data, 24, nameLen);
            else
                strName = null;


            //This check makes sure that strName has been passed in the array of bytes
            if (strMessageLen > 0)
                strMessage = Encoding.UTF8.GetString(data, 24 + nameLen, strMessageLen);
            else
                strMessage = null;

            //This checks for a null message field
            if (strMessage2Len > 0)
                strMessage2 = Encoding.UTF8.GetString(data, 24 + nameLen + strMessageLen, strMessage2Len);
            else
                strMessage2 = null;

            if (strMessage3Len > 0)
                strMessage3 = Encoding.UTF8.GetString(data, 24 + nameLen + strMessageLen + strMessage2Len, strMessage3Len);
            else
                strMessage3 = null;

            if (strMessage4Len > 0)
                strMessage4 = Encoding.UTF8.GetString(data, 24 + nameLen + strMessageLen + strMessage2Len + strMessage3Len, strMessage4Len);
            else
                strMessage4 = null;
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

            //Length of the message
            if (strMessage != null)
                result.AddRange(BitConverter.GetBytes(strMessage.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Length of the message2
            if (strMessage2 != null)
                result.AddRange(BitConverter.GetBytes(strMessage2.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Length of the message3
            if (strMessage3 != null)
                result.AddRange(BitConverter.GetBytes(strMessage3.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Length of the message4
            if (strMessage4 != null)
                result.AddRange(BitConverter.GetBytes(strMessage4.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Add the name
            if (strName != null)
                result.AddRange(Encoding.UTF8.GetBytes(strName));

            //Add the message
            if (strMessage != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage));

            //And, lastly we add the message text to our array of bytes
            if (strMessage2 != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage2));

            if (strMessage3 != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage3));

            if (strMessage4 != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage4));

            return result.ToArray();
        }
    }
}

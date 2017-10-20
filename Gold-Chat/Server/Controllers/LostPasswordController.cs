using CommandClient;
using Server.Interfaces;
using Server.Interfaces.LostPassword;
using Server.Modules.ResponseMessagesController;
using System;
using System.Collections.Generic;

namespace Server.Controllers
{
    class LostPasswordController : ServerResponds, IBuildResponse
    {
        private readonly IInsertUserLostPasswordCode InsertToLostPasswordTable;
        private readonly ISendEmailWithPasswordCode SendLostPasswrdCodeToEmail;
        private readonly IGetDataUsingCode GetDataUsingCode;
        private readonly IGetUserIdAndEmail GetUserIdAndEmail;
        private readonly IDeleteLostPasswordCode DeleteLostPasswordCodeFromDB;
        private readonly IUpdateNewPassword UpdateNewPassword;

        public LostPasswordController(IInsertUserLostPasswordCode insertToLostPasswordTable, ISendEmailWithPasswordCode sendLostPasswrdCodeToEmail,
            IGetDataUsingCode getDataUsingCode, IGetUserIdAndEmail getUserIdAndEmail, IDeleteLostPasswordCode deleteLostPasswordCodeFromDB, IUpdateNewPassword UpdatePassword)
        {
            InsertToLostPasswordTable = insertToLostPasswordTable;
            SendLostPasswrdCodeToEmail = sendLostPasswrdCodeToEmail;
            GetDataUsingCode = getDataUsingCode;
            GetUserIdAndEmail = getUserIdAndEmail;
            DeleteLostPasswordCodeFromDB = deleteLostPasswordCodeFromDB;
            UpdateNewPassword = UpdatePassword;
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
        }

        public void Execute()
        {
            prepareResponse();
            string type = Received.strMessage;

            if (type == "email")
                clientWantNewPasswrd();
            else if (type == "codeFromEmail")
                clientSendCodeFromEmail();
            else Send.strMessage = "Wrong operation option";
        }

        private void clientWantNewPasswrd()
        {
            string[] respond = GetUserIdAndEmail.Get(Received.strMessage2);
            if (respond != null)
            {
                string email = respond[1];
                string generatedCode = generateRandom(25);

                if (InsertToLostPasswordTable.InsertCode(respond[0], generatedCode) > 0)
                {
                    if (SendLostPasswrdCodeToEmail.SendLostPasswordMessage(email, generatedCode))
                        Send.strMessage = "Lost password code has send to your email";
                }
                else Send.strMessage = "Unknown error while save random code, contact to admin";
            }
            else Send.strMessage = "That email not exists";
        }

        private void clientSendCodeFromEmail()
        {
            string code = Received.strMessage2;
            string newPassword = Received.strMessage3;

            string[] codeDb = GetDataUsingCode.GetData(code);
            if (codeDb != null && codeDb[0] == code)
            {
                if (DeleteLostPasswordCodeFromDB.Delete(code) == 0)
                    Console.WriteLine("Cannot delete " + codeDb[1] + " from user_lost_pass");

                Send.strMessage = UpdateNewPassword.Update(newPassword, codeDb[3], codeDb[2]);
            }
            else
                Send.strMessage = "Wrong code from email";
        }

        private string generateRandom(int lenght)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var stringChars = new char[lenght];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
                stringChars[i] = chars[random.Next(chars.Length)];

            return new string(stringChars);
        }
    }
}

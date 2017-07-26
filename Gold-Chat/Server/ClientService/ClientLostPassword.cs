using CommandClient;
using Server.Interfaces;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientLostPassword : ServerResponds, IBuildResponse
    {
        DataBaseManager db = DataBaseManager.Instance;
        EmailSender emailSender = EmailSender.Instance;

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
            {
                string emailAdr = Received.strMessage2;

                db.bind("Email", emailAdr);
                db.manySelect("SELECT id_user, email FROM users WHERE email = @Email");
                string[] respond = db.tableToRow();
                if (respond != null)
                {
                    string email = respond[1];
                    string generatedCode = generateRandom(25);
                    if (inserUserLostPasswordCodeToDb(respond, generatedCode) > 0)
                        sendEmailOnUserLostPassword(email, generatedCode);
                    else Send.strMessage = "Unknown error while save random code, contact to admin";
                }
                else Send.strMessage = "That email not exists";
            }
            else if (type == "codeFromEmail")
                clientSendCodeFromEmail();
            else Send.strMessage = "Wrong operation option";
        }

        private int inserUserLostPasswordCodeToDb(string[] respond, string generatedCode)
        {
            int id_user = Int32.Parse(respond[0]);

            db.bind(new string[] { "idUser", id_user.ToString(), "Code", generatedCode, "CodeCreateDate", Utilities.getDataTimeNow() });
            int created = db.executeNonQuery("INSERT INTO user_lost_pass (id_user, code, code_create_date) " + "VALUES (@idUser, @Code, @CodeCreateDate)");
            return created;
        }

        private void sendEmailOnUserLostPassword(string email, string generatedCode)
        {
            emailSender.SendEmail("Lost Password", email, "Gold Chat: Lost Password", userLostPassEmailMessage("User", generatedCode));
            Send.strMessage = "Lost password code has send to your email";
        }

        private void clientSendCodeFromEmail()
        {
            string code = Received.strMessage2;
            string newPassword = Received.strMessage3;

            db.bind("Code", code);
            db.manySelect("SELECT ulp.code, u.email, u.password, u.login FROM user_lost_pass ulp, users u WHERE u.id_user = ulp.id_user AND code = @Code");
            string[] codeDb = db.tableToRow();
            if (codeDb != null && codeDb[0] == code)
            {
                db.bind("Code", code);
                int deleted = db.executeNonQuery("DELETE FROM user_lost_pass WHERE code = @Code");

                if (deleted == 0)
                    Console.WriteLine("Cannot delete " + codeDb[1] + " from user_lost_pass");

                string updated = updateUserPasswordToDb(newPassword, codeDb[3], codeDb[2]);
                Send.strMessage = updated;
            }
            else
                Send.strMessage = "Wrong code from email";
        }

        private string updateUserPasswordToDb(string newPassword, string userName, string oldPassword)
        {
            db.bind(new string[] { "pass", newPassword, "Login", userName, "oldPass", oldPassword });
            if (db.executeNonQuery("UPDATE users SET password = @pass WHERE login = @Login AND password = @oldPass") > 0)
                return "Your Password has been changed!";
            else
                return "Unknow Error while changing password";
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
    }
}

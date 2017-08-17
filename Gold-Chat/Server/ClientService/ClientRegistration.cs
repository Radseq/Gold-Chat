using CommandClient;
using Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Server.ClientService
{
    class ClientRegistration : ServerResponds, IBuildResponse
    {
        public event EventHandler<ClientEventArgs> ClientRegistrationEvent;

        DataBaseManager db = DataBaseManager.Instance;
        EmailSender emailSender = new EmailSender();

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
        }

        public void Execute()
        {
            prepareResponse();
            string userName = Received.strName;
            string userPassword = Received.strMessage;
            string userEmail = Received.strMessage2;

            string[] query = GetDataFromDB(userName, userEmail);
            if (query != null)
            {
                if (query[0] == userName)
                    Send.strMessage = "Your login exists, try other one";
                else if (query[1] == userEmail)
                    Send.strMessage = "Your email exists, try other one";
                else if (query[2] != "")
                    Send.strMessage = "You have already register, on next login you will be ask for register key";
                else
                    insertUserToDb(userName, userEmail, userPassword);
            }
            else insertUserToDb(userName, userEmail, userPassword);
        }

        private string[] GetDataFromDB(string userName, string userEmail)
        {
            db.bind(new string[] { "Login", userName, "Email", userEmail });
            db.manySelect("SELECT login, email, register_id FROM users WHERE login = @Login OR email = @Email");
            return db.tableToRow();
        }

        private void insertUserToDb(string userName, string userEmail, string userPassword)
        {
            string registrationCode = CalculateChecksum(userEmail);

            db.bind(new string[] { "user_name", userName, "user_password", userPassword, "user_email", userEmail, "register_id", registrationCode, "perm", 0.ToString() });
            int created = db.executeNonQuery("INSERT INTO users(login, password, email, register_id, permission) " + "VALUES(@user_name, @user_password, @user_email, @register_id, @perm)");

            if (created > 0)
            {
                SentRegistrationCodeToEmail(userName, userEmail, registrationCode);
                Send.strMessage = "You has been registered";
                OnClientRegistration(userName, userEmail);
            }
            else
                Send.strMessage = "Account NOT created with unknown reason.";
        }

        private void SentRegistrationCodeToEmail(string userName, string userEmail, string registrationCode)
        {
            emailSender.SetProperties(userName, userEmail, "Gold Chat: Registration", userRegistrationMessage(userName, registrationCode));
            emailSender.SendEmail();
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

        //must be in new class
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

        protected virtual void OnClientRegistration(string cName, string cEmail)
        {
            ClientRegistrationEvent?.Invoke(this, new ClientEventArgs() { clientName = cName, clientEmail = cEmail });
        }
    }
}

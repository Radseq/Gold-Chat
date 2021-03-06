﻿using Server.Interfaces;
using Server.Interfaces.ClientLogin;
using System;

namespace Server.ClientLoginModule
{
    class SendLoginNotyfiToEmail : ISendLoginNotyfication
    {
        public readonly ISendEmail SendEmail;

        public SendLoginNotyfiToEmail(ISendEmail sendEmail)
        {
            SendEmail = sendEmail;
        }

        public bool SendLoginNotyfication(string userName, string destination)
        {
            SendEmail.SetProperties(userName, destination, "Gold Chat: Login Notyfication", "You have login: " + DateTime.Now.ToString("dd:MM On HH:mm:ss") + " To Gold Chat Account.");
            return SendEmail.SendEmail();
        }
    }
}

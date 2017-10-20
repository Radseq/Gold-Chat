using Server.Interfaces;
using Server.Interfaces.LostPassword;

namespace Server.Modules.LostPasswordModule
{
    class SendEmailLostPasswordCode : ISendEmailWithPasswordCode
    {

        private readonly ISendEmail Send;
        private readonly ILostPasswordEmailMessage EmailMessage;

        public SendEmailLostPasswordCode(ISendEmail sendEmail, ILostPasswordEmailMessage emailMsg)
        {
            Send = sendEmail;
            EmailMessage = emailMsg;
        }

        public bool SendLostPasswordMessage(string email, string generatedCode)
        {
            Send.SetProperties("Lost Password", email, "Gold Chat: Lost Password", EmailMessage.lostPassEmailMessage("User", generatedCode));
            return Send.SendEmail();
        }
    }
}

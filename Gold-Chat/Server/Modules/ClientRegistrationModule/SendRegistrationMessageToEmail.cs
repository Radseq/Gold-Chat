using Server.Interfaces;
using Server.Interfaces.ClientRegistration;

namespace Server.Modules.ClientRegistrationModule
{
    class SendRegistrationMessageToEmail : ISendRegistrationMessage
    {
        public readonly ISendEmail SendEmail;
        public readonly IRegistrationMessage RegisterMessage;

        public SendRegistrationMessageToEmail(ISendEmail sendEmail, IRegistrationMessage registerMessage)
        {
            SendEmail = sendEmail;
            RegisterMessage = registerMessage;
        }

        //iam not sure but email message should be there i guess -> from RegistrationEmailMessage class

        public bool Send(string userName, string destination, string code)
        {
            SendEmail.SetProperties(userName, destination, "Gold Chat: Registration", RegisterMessage.RegistrationMessage(userName, code));
            return SendEmail.SendEmail();
        }
    }
}

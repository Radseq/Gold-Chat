using Server.Interfaces;
using Server.Interfaces.ClientSendActiveCode;

namespace Server.Modules.ActiveCodeModule
{
    public class SendActivactionCodeToUserEmail : ISendActivationCode
    {
        public readonly ISendEmail SendEmail;

        public SendActivactionCodeToUserEmail(ISendEmail sendEmail)
        {
            SendEmail = sendEmail;
        }

        public bool Send(string UserName, string destination, string code)
        {
            SendEmail.SetProperties(UserName, destination, "Gold Chat: Resended Register Code", "Here is your activation code: " + code);
            return SendEmail.SendEmail();
        }
    }
}

namespace Server.Interfaces
{
    public interface ISendEmail
    {
        void SetProperties(string userName, string toEmail, string subject, string emailMessage);
        bool SendEmail();
    }
}

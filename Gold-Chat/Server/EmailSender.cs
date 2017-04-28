using System;
using System.Net.Mail;

namespace Server
{
    public class EmailSenderEventArgs : EventArgs
    {
        public string Subject { get; set; }
        public string ToEmail { get; set; }
        public string EmailMessage { get; set; }
        public string UserNameEmail { get; set; }
    }

    class EmailSender
    {
        // Singleton
        static EmailSender instance = null;
        static readonly object padlock = new object();

        public event EventHandler<EmailSenderEventArgs> EmailSended;
        public event EventHandler<EmailSenderEventArgs> FailEmailSended;

        // Singleton
        public static EmailSender Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new EmailSender();

                    return instance;
                }
            }
        }

        public void SendEmail(string userName, string toEmail, string subject, string emailMessage)
        {
            string fromAddress = "atlantiss.chat@gmail.com";

            MailMessage message = new MailMessage(fromAddress, toEmail, subject, emailMessage);

            message.IsBodyHtml = true;

            //Set Priority of the Email
            message.Priority = MailPriority.High;

            SmtpClient emailClient = new SmtpClient("smtp.gmail.com", 587);

            //Set EnableSsl = True as gmail requires SSL
            emailClient.EnableSsl = true;

            //Credentials for the sender
            emailClient.Credentials = new System.Net.NetworkCredential(fromAddress, "atlanprogramemail");

            try
            {
                //Send the Email
                emailClient.Send(message);
                OnEmailSended(userName, subject, toEmail, emailMessage);
            }
            catch
            {
                OnFailEmailSended(userName, subject, toEmail, emailMessage);
            }
        }

        protected virtual void OnEmailSended(string userName, string toEmail, string subject, string emailMessage)
        {
            EmailSended?.Invoke(this, new EmailSenderEventArgs() { UserNameEmail = userName, ToEmail = toEmail, Subject = subject, EmailMessage = emailMessage });
        }

        protected virtual void OnFailEmailSended(string userName, string toEmail, string subject, string emailMessage)
        {
            FailEmailSended?.Invoke(this, new EmailSenderEventArgs() { UserNameEmail = userName, ToEmail = toEmail, Subject = subject, EmailMessage = emailMessage });
        }
    }
}

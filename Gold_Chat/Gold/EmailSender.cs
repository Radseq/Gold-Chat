using System;
using System.Net.Mail;

namespace Gold
{
    public class EmailSenderEventArgs : EventArgs
    {
        public string Subject { get; set; }
        public string ToEmail { get; set; }
        public string EmailMessage { get; set; }
    }

    class EmailSender
    {
        public event EventHandler<EmailSenderEventArgs> EmailSended;
        public event EventHandler<EmailSenderEventArgs> FailEmailSended;

        public void SendEmail(string toEmail, string subject, string emailMessage)
        {
            string fromAddress = "atlantiss.chat@gmail.com";

            //string content = String.Format("Report Send By: {0} {1} Topic: {2} {1} issue: {3}", App.clientName, Environment.NewLine, topic, messageToClient);
            // /n nie daje dowej lini.........
            // ani Environment.NewLine
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
                OnEmailSended(subject, toEmail, emailMessage);
            }
            catch
            {
                OnFailEmailSended(subject, toEmail, emailMessage);
            }
        }

        protected virtual void OnEmailSended(string toEmail, string subject, string emailMessage)
        {
            EmailSended?.Invoke(this, new EmailSenderEventArgs() { ToEmail = toEmail, Subject = subject, EmailMessage = emailMessage });
        }

        protected virtual void OnFailEmailSended(string toEmail, string subject, string emailMessage)
        {
            FailEmailSended?.Invoke(this, new EmailSenderEventArgs() { ToEmail = toEmail, Subject = subject, EmailMessage = emailMessage });
        }
    }
}

using System;
//for mail(suggestions)
using System.Windows;
using System.Windows.Controls;

namespace Gold.tab_windows
{
    /// <summary>
    /// Interaction logic for p_suggest.xaml
    /// </summary>
    public partial class p_suggest : UserControl
    {
        public p_suggest()
        {
            InitializeComponent();
        }
        private void sendmail_click(object sender, RoutedEventArgs e)
        {
            string topic = topicTextBox.Text;
            string content = string.Format("Report Send By: {0} {1} Topic: {2} {1} issue: {3}", App.clientName, Environment.NewLine, topic, messegeTextBox.Text);

            var emailSender = new EmailSender();
            emailSender.EmailSended += OnEmaiSended;
            emailSender.SendEmail("atlantiss.chat@gmail.com", "Sugestion by: " + App.clientName, content);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OnEmaiSended(object source, EmailSenderEventArgs args)
        {
            MessageBox.Show("Thenks for suggestions", "Close", MessageBoxButton.OK);
            topicTextBox.Text = "";
            messegeTextBox.Text = "";
        }
    }
}

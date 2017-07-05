using System;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gold_Client.ProgramableWindow
{
    class GenerateChannelPassWindow
    {
        public event EventHandler<SecureString> OnClickOrEnter;

        PasswordBox pb;

        public void createWindow(string windowTitle, string windowLabel)
        {
            CreateWindow programableWindow = new CreateWindow(windowTitle);
            programableWindow.changeLabelContent(windowLabel);
            pb = programableWindow.addPasswordBoxToWindow("pass1", new Thickness(10, 43, 10, 10));
            pb.PreviewKeyDown += EnterClicked;
            Button button = programableWindow.addButton("button", "send", new Thickness(202, 71, 10, 10));
            button.Click += new RoutedEventHandler(button_Click);
            programableWindow.Show();
        }

        void EnterClicked(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                OnClickOrEnter?.Invoke(this, (sender as PasswordBox).SecurePassword);
                e.Handled = true;
            }
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            OnClickOrEnter?.Invoke(this, pb.SecurePassword);
        }
    }
}

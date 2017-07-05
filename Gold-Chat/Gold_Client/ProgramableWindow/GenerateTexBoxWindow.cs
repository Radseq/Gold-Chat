using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gold_Client.ProgramableWindow
{
    class GenerateTexBoxWindow
    {
        public event EventHandler<string> OnClickOrEnter;

        public void createWindow(string windowTitle, string windowLabel)
        {
            CreateWindow programableWindow = new CreateWindow(windowTitle);
            programableWindow.changeLabelContent(windowLabel);
            TextBox tb = programableWindow.addTextBoxToWindow("textbox", new Thickness(10, 43, 10, 10));
            tb.PreviewKeyDown += EnterClicked;
            Button button = programableWindow.addButton("button", "send", new Thickness(202, 71, 10, 10));
            button.Click += new RoutedEventHandler(button_Click);
            programableWindow.Show();
        }

        void EnterClicked(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                OnClickOrEnter?.Invoke(this, (sender as TextBox).Text);
                e.Handled = true;
            }
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            OnClickOrEnter?.Invoke(this, (sender as TextBox).Text);
        }
    }
}

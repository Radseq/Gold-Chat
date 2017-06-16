using System.Windows;
using System.Windows.Controls;

namespace Gold_Client.ViewModel.Others
{
    class ServerAskClient : Window
    {
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;

        Grid grid = new Grid();
        Label label = new Label();

        private TextBox textBox = new TextBox();
        private PasswordBox passwordBox = new PasswordBox();

        public ServerAskClient(string nameOfWindow)
        {
            Topmost = true;
            Title = nameOfWindow;
            Width = 290;
            Height = 150;
            label.Margin = new Thickness(10, 10, 10, 71);
            grid.Children.Add(label);
            Content = grid;
        }

        public TextBox addTextBoxToWindow(string textboxName)
        {
            textBox.Height = 23;
            textBox.Width = 262;
            textBox.Margin = new Thickness(10, 43, 10, 10);
            textBox.Name = textboxName;
            grid.Children.Add(textBox);
            return textBox;
        }

        public void changeLabelContent(string contentText)
        {
            label.Content = contentText;
        }

        public Button addButton(string buttonName, string buttonContent)
        {
            Button button = new Button();
            button.Name = buttonName;
            button.Content = buttonContent;
            button.Margin = new Thickness(202, 71, 10, 10);
            grid.Children.Add(button);
            return button;
        }

        public PasswordBox addPasswordBoxToWindow(string passwordBoxName)
        {
            passwordBox.Height = 23;
            passwordBox.Width = 262;
            passwordBox.Margin = new Thickness(10, 43, 10, 10);
            passwordBox.Name = passwordBoxName;
            grid.Children.Add(passwordBox);
            return passwordBox;
        }

        public void deleteWindowElement(string elementName)
        {
            if (elementName != null)
                grid.Children.Remove((UIElement)FindName(elementName));
        }
    }
}

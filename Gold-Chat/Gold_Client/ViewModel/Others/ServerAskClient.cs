using CommandClient;
using System.Windows;
using System.Windows.Controls;

namespace Gold_Client.ViewModel.Others
{
    class ServerAskClient : Window
    {
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;

        //private Canvas canvas = new Canvas();
        Grid grid = new Grid();
        Label label = new Label();

        private TextBox textBox = new TextBox();
        private PasswordBox passwordBox = new PasswordBox();

        string ClientNameInChannel;

        public ServerAskClient(string nameOfWindow, string clientNameInChannel)
        {
            ClientNameInChannel = clientNameInChannel;

            //AllowsTransparency = true;
            // WindowStyle = WindowStyle.None;
            // Background = Brushes.Black;
            Topmost = true;
            Title = nameOfWindow;
            Width = 290;
            Height = 150;
            //canvas.Width = Width;
            //canvas.Height = Height;
            //canvas.Background = Brushes.Black;
            label.Margin = new Thickness(10, 10, 10, 71);
            grid.Children.Add(label);
            Content = grid;
        }

        public void addTextBoxToWindow(string textboxName)
        {
            textBox.Height = 23;
            textBox.Width = 262;
            textBox.Margin = new Thickness(10, 43, 10, 10);
            textBox.Name = textboxName;
            textBox.PreviewKeyDown += EnterClicked;
            grid.Children.Add(textBox);

        }

        public void changeLabelContent(string contentText)
        {
            label.Content = contentText;
        }

        public void addButton(string buttonName, string buttonContent)
        {
            Button button = new Button();
            button.Name = buttonName;
            button.Content = buttonContent;
            button.Margin = new Thickness(202, 71, 10, 10);
            button.Click += new RoutedEventHandler(button_Click);
            grid.Children.Add(button);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            sendPasswordToServer();
        }

        private string getMessage()
        {
            if (textBox.Name != "")
            {
                return textBox.Text;
            }
            else if (passwordBox.Name != "")
            {
                return passwordBox.Password.ToString();
            }
            else
                MessageBox.Show("Please insert a text/password", "Error validation", MessageBoxButton.OK, MessageBoxImage.Information);
            return null;
        }

        void EnterClicked(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                sendPasswordToServer();
                e.Handled = true;
            }
        }

        private void sendPasswordToServer()
        {
            clientSendToServer.SendToServer(Command.joinChannel, ClientNameInChannel, clientSendToServer.CalculateChecksum(getMessage()));

            Close();
        }

        public void addPasswordBoxToWindow(string passwordBoxName)
        {
            passwordBox.Height = 23;
            passwordBox.Width = 262;
            passwordBox.Margin = new Thickness(10, 43, 10, 10);
            passwordBox.Name = passwordBoxName;
            passwordBox.PreviewKeyDown += EnterClicked;
            grid.Children.Add(passwordBox);
        }

        public void deleteWindowElement(string elementName)
        {
            if (elementName != null)
                grid.Children.Remove((UIElement)FindName(elementName));
        }
    }
}

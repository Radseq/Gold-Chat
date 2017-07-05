using Gold_Client.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Gold_Client.ProgramableWindow
{
    class CreateWindow : Window
    {
        static ClientSendToServer clientSendToServer = ClientSendToServer.Instance;

        Grid grid = new Grid();
        Label label = new Label();

        private TextBox textBox = new TextBox();
        private PasswordBox passwordBox = new PasswordBox();

        public CreateWindow(string nameOfWindow)
        {
            Topmost = true;
            Title = nameOfWindow;
            Width = 290;
            Height = 200;
            label.Margin = new Thickness(10, 10, 10, 71);
            grid.Children.Add(label);
            Content = grid;
        }

        public TextBox addTextBoxToWindow(string textboxName, Thickness pos)
        {
            textBox.Height = 23;
            textBox.Width = 262;
            textBox.Margin = pos;
            textBox.Name = textboxName;
            grid.Children.Add(textBox);
            return textBox;
        }

        public void changeLabelContent(string contentText)
        {
            label.Content = contentText;
        }

        public Button addButton(string buttonName, string buttonContent, Thickness pos)
        {
            Button button = new Button();
            button.Name = buttonName;
            button.Content = buttonContent;
            button.Margin = pos;
            grid.Children.Add(button);
            return button;
        }

        public PasswordBox addPasswordBoxToWindow(string passwordBoxName, Thickness pos)
        {
            passwordBox.Height = 23;
            passwordBox.Width = 262;
            passwordBox.Margin = pos;
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

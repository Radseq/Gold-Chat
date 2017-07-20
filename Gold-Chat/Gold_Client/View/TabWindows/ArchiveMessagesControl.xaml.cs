using System.Windows;

namespace Gold_Client.View.TabWindows
{
    public partial class ArchiveMessagesControl
    {
        /* TODO
         * kazda linijka pliku zawiera id_urzytkownika, nazwa uzytkownika, data wyslania wiadomosci, wiadomosc
         * po kliknieciu na item listy(nazwa urzytkownika+data) tresc wiadomosci pokazywana jest w message_content
         */

        public ArchiveMessagesControl()
        {
            InitializeComponent();
        }

        private void users_Loaded(object sender, RoutedEventArgs e)
        {
            //user_list.Items.Add();

        }

        private void ListBoxItem_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // pokazanie wiadomosci w message_content
        }
    }
}

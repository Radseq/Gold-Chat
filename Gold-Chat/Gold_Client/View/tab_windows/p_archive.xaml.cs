using System.Windows;

namespace Gold_Client.View.tab_windows
{
    /// <summary>
    /// Interaction logic for archive.xaml
    /// </summary>
    public partial class p_archive
    {
        /* TODO
         * pobieranie rozmowy z pliku txt lub bazy sql
         * kazda linijka pliku zawiera id_urzytkownika, nazwa uzytkownika, data wyslania wiadomosci, wiadomosc
         * po kliknieciu na item listy(nazwa urzytkownika+data) tresc wiadomosci pokazywana jest w message_content
         */

        public p_archive()
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

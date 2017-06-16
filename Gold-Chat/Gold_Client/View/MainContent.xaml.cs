using Gold_Client.ViewModel;
using System.Windows.Controls;

namespace Gold_Client.View
{
    /// <summary>
    /// Interaction logic for MainContent.xaml
    /// </summary>
    public partial class MainContent : UserControl
    {
        public MainContent()
        {
            MainContentPresenter presenter = new MainContentPresenter();
            DataContext = presenter;
            InitializeComponent();
        }
    }
}

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
            //MainContentPresenter presenter = new MainContentPresenter();
            //DataContext = presenter;
            // DataContext is added in MainProgramWindow.xaml file line:20
            InitializeComponent();
        }
    }
}

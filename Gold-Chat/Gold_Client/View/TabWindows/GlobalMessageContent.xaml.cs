using Gold_Client.ViewModel.TabWindows;
using System.Windows.Controls;

namespace Gold_Client.View.TabWindows
{

    public partial class GlobalMessageContent : UserControl
    {
        public GlobalMessageContent()
        {
            DataContext = new GlobalMessageContentPresenter();
            InitializeComponent();
        }
    }
}

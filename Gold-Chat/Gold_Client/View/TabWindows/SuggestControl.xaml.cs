using Gold_Client.ViewModel.TabWindows;

namespace Gold_Client.View.TabWindows
{
    public partial class SuggestControl
    {
        public SuggestControl()
        {
            DataContext = new UserSuggestPresenter();
            InitializeComponent();
        }
    }
}

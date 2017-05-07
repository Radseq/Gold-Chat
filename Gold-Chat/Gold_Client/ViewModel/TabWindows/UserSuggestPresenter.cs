using System.Windows.Input;

namespace Gold_Client.ViewModel.TabWindows
{
    public class UserSuggestPresenter : ObservableObject
    {
        private string topicSuggestMessageTB;
        private string suggestMessageTB;

        public ICommand SendSuggest => new DelegateCommand(() =>
        {

        });

        public string TopicSuggestMessageTB
        {
            get { return topicSuggestMessageTB; }
            set
            {
                topicSuggestMessageTB = value;
                RaisePropertyChangedEvent(nameof(TopicSuggestMessageTB));
            }
        }

        public string SuggestMessageTB
        {
            get { return suggestMessageTB; }
            set
            {
                suggestMessageTB = value;
                RaisePropertyChangedEvent(nameof(SuggestMessageTB));
            }
        }
    }
}

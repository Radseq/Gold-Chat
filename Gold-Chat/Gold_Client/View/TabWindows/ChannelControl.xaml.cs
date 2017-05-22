namespace Gold_Client.View.TabWindows
{
    public partial class ChannelControl
    {
        public ChannelControl(string ChannelName, string WelcomeChannelMsg)
        {
            InitializeComponent();
            if (DataContext != null)
            {
                ((dynamic)DataContext).channelName = ChannelName;
                ((dynamic)DataContext).WelcomeChannelMsg = WelcomeChannelMsg;
            }
        }
    }
}

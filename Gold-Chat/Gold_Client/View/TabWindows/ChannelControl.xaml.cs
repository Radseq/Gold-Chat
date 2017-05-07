namespace Gold_Client.View.TabWindows
{
    public partial class ChannelControl
    {
        public ChannelControl(string ChannelName)
        {
            InitializeComponent();
            if (DataContext != null)
            { ((dynamic)DataContext).channelName = ChannelName; }
        }
    }
}

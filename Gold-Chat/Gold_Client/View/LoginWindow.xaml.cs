using System;

namespace Gold_Client.View
{
    public partial class LoginWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
            //if (DataContext != null)
            //{ ((dynamic)DataContext).loginWindow = this; }
            if (DataContext != null)
            { ((dynamic)DataContext).CloseAction = new Action(() => DialogResult = true); }
        }
    }
}

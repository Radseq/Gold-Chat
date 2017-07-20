using Gold_Client.ViewModel;
using System;

namespace Gold_Client.View
{
    public partial class RegistrationWindow
    {
        public RegistrationWindow(LoginPresenter login)
        {
            InitializeComponent();
            if (DataContext != null)
            {
                ((dynamic)DataContext).loginPresenter = login;
                if (((dynamic)DataContext).CloseAction == null)
                    ((dynamic)DataContext).CloseAction = new Action(Close);
            }
        }
    }
}

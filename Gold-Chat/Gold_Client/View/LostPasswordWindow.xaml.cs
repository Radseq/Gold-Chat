using System;

namespace Gold_Client.View
{
    public partial class LostPasswordWindow
    {
        public LostPasswordWindow()
        {
            InitializeComponent();
            if (DataContext != null)
            {
                if (((dynamic)DataContext).CloseAction == null)
                    ((dynamic)DataContext).CloseAction = new Action(Close);
            }
        }
    }
}

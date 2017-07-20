using System;

namespace Gold_Client.View
{

    public partial class MainProgramWindow
    {
        public MainProgramWindow()
        {
            //DataContext = new MainWindowPresenter();
            InitializeComponent();
            if (DataContext != null)
            {
                if (((dynamic)DataContext).CloseAction == null)
                    ((dynamic)DataContext).CloseAction = new Action(Close);
            }
        }
    }
}

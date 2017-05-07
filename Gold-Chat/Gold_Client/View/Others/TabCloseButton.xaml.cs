using System;
using System.Windows;
using System.Windows.Controls;

namespace Gold_Client.View.Others
{
    public partial class TabCloseButton : UserControl
    {
        public event EventHandler Click;

        public TabCloseButton()
        {
            InitializeComponent();
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(GetParents(sender, 0));
            Click?.Invoke(sender, e);
        }

        //private string GetParents(Object element, int parentLevel)
        //{
        //    string returnValue = String.Format("[{0}] {1}", parentLevel, element.GetType());
        //    if (element is FrameworkElement)
        //    {
        //        if (((FrameworkElement)element).Parent != null)
        //            returnValue += String.Format("{0}{1}",
        //                Environment.NewLine, GetParents(((FrameworkElement)element).Parent, parentLevel + 1));
        //    }
        //    return returnValue;
        //}
    }
}

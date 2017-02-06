using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gold.tab_windows
{
    /// <summary>
    /// Interaction logic for tabcontrol.xaml
    /// </summary>
    public partial class p_admin : UserControl
    {

        public p_admin()
        {
            InitializeComponent();
        }
        private void BackgroundRedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateRectangleColors();
        }

        private void BackgroundGreenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateRectangleColors();
        }

        private void BackgroundBlueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateRectangleColors();
        }

        private void UpdateRectangleColors()
        {

            Color clr = Color.FromArgb(255, Convert.ToByte(RedSlider.Value),
                Convert.ToByte(GreenSlider.Value), Convert.ToByte(BlueSlider.Value));

            rectangle1.Fill = new SolidColorBrush(clr);
        }

        private void ForegroundRedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateLaberColors();
        }

        private void ForegroundGreenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateLaberColors();
        }

        private void ForegroundBlueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateLaberColors();
        }

        private void UpdateLaberColors()
        {
            Color clr = Color.FromArgb(255, Convert.ToByte(ForegroundRedSlider.Value),
                Convert.ToByte(ForegroundGreenSlider.Value), Convert.ToByte(ForegroundBlueSlider.Value));

            label1.Foreground = new SolidColorBrush(clr);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

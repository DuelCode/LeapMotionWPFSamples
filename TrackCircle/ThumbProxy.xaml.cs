using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrackCircle
{
    /// <summary>
    /// Thumb.xaml 的交互逻辑
    /// </summary>
    public partial class ThumbProxy : UserControl
    {
        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(ThumbProxy), new UIPropertyMetadata(0.0, XPropertyChanged));

        private static void XPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ThumbProxy oItem = obj as ThumbProxy;
            oItem.TbkLocation.Text = (int)(double)e.NewValue + " " + (int)oItem.Y;
        }

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(ThumbProxy), new UIPropertyMetadata(0.0, YPropertyChanged));

        private static void YPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ThumbProxy oItem = obj as ThumbProxy;
            oItem.TbkLocation.Text = (int)oItem.X + " " + (int)(double)e.NewValue;
        }

        public SolidColorBrush BackgroundColor
        {
            get { return (SolidColorBrush)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public static readonly DependencyProperty BackgroundColorProperty =
          DependencyProperty.Register("BackgroundColor", typeof(SolidColorBrush), 
              typeof(ThumbProxy), new UIPropertyMetadata(new SolidColorBrush(Colors.Blue), BackgroundColorPropertyChanged));

        private static void BackgroundColorPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ThumbProxy oItem = obj as ThumbProxy;
            oItem.EllipseMain.Fill = (SolidColorBrush)e.NewValue;
        }

        public ThumbProxy()
        {
            InitializeComponent();
            
            this.DataContext = this;
        }
    }
}

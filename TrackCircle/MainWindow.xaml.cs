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
using Leap;
using System.Diagnostics;
using TrackCircle.Core;

namespace TrackCircle
{
    public partial class MainWindow : Window
    {
        private ThumbProxy FingerThumb;
        private ThumbProxy HandThumb;

        private Controller LeapController;
        private LeapListener LeapListener;


        public MainWindow()
        {
            InitializeComponent();

            this.LeapController = new Controller();
            this.LeapListener = new LeapListener();
            this.LeapController.AddListener(this.LeapListener);
            this.LeapListener.OnFingerLocationChanged += LeapListener_OnFingerLocationChanged;
            this.LeapListener.OnHandLocationChanged += LeapListener_OnHandLocationChanged;

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        private void LeapListener_OnHandLocationChanged(double dX, double dY)
        {
            this.Dispatcher.Invoke(new Action(() => {
                if (this.HandThumb != null)
                {
                    this.HandThumb.X = dX;
                    this.HandThumb.Y = dY;
                }
            }));
        }

        private void LeapListener_OnFingerLocationChanged(double dX, double dY)
        {
            this.Dispatcher.Invoke(new Action(() => {
                if (this.FingerThumb != null)
                {
                    this.FingerThumb.X = dX;
                    this.FingerThumb.Y = dY;
                }
            }));
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        { 
            this.Closing -= MainWindow_Closing;
            this.LeapController.RemoveListener(this.LeapListener);
            this.LeapController.Dispose();
            this.LeapListener.Dispose();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= MainWindow_Loaded;

            this.FingerThumb = new ThumbProxy();
            this.FingerThumb.X = (this.CvMain.ActualWidth- this.FingerThumb.Width) / 2d;
            this.FingerThumb.Y = (this.CvMain.ActualHeight - this.FingerThumb.Height) / 2d;
            this.CvMain.Children.Add(this.FingerThumb);

            this.HandThumb = new ThumbProxy();
            this.HandThumb.BackgroundColor = new SolidColorBrush(Colors.Red);
            this.HandThumb.X = (this.CvMain.ActualWidth - this.HandThumb.Width) / 2d;
            this.HandThumb.Y = (this.CvMain.ActualHeight - this.HandThumb.Height) / 2d;
            this.CvMain.Children.Add(this.HandThumb);
        }
    }
}

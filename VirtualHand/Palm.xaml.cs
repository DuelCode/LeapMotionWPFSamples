using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;

namespace VirtualHand
{
    /// <summary>
    /// Hand.xaml 的交互逻辑
    /// </summary>
    public partial class Palm : UserControl
    {
        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(Palm), new UIPropertyMetadata(0.0));

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(Palm), new UIPropertyMetadata(0.0));

        public Palm()
        {
            InitializeComponent();
            this.InitUI();
            this.DataContext = this;
            this.Loaded += Palm_Loaded;
        }

        private void Palm_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Palm_Loaded;

            DispatcherTimer oTimer = new DispatcherTimer();
            oTimer.Interval = TimeSpan.FromSeconds(3);
            oTimer.Tick += (s1, e1) => {
                RevertFingerState();
            };
            oTimer.Start();
        }

        private void InitUI()
        {
            for (int i = 0; i < this.GdTips.Children.Count; i++)
            {
                TipItem oItem = this.GdTips.Children[i] as TipItem;
                if (oItem != null)
                {
                    double dMarginBottom = (this.BdrPalm.Height - oItem.Height) / 2d;
                    oItem.Margin = new Thickness(0, 0, 0, dMarginBottom);
                    oItem.ScaleX = 0.5;
                    oItem.ScaleY = 0.5;
                }
            }
        }

        public void UpdatePalmState(Finger oFinfer, Leap.Vector vecterFingerTip, Leap.Vector vecterPalm)
        {
            if (oFinfer.Type == Finger.FingerType.TYPE_THUMB)
            {
                this.TipThumb.UpdateFingerState(vecterFingerTip, vecterPalm, oFinfer);
            }
            else if (oFinfer.Type == Finger.FingerType.TYPE_INDEX)
            {
                this.TipIndex.UpdateFingerState(vecterFingerTip, vecterPalm, oFinfer);
            }
            else if (oFinfer.Type == Finger.FingerType.TYPE_MIDDLE)
            {
                this.TipMiddle.UpdateFingerState(vecterFingerTip, vecterPalm, oFinfer);
            }
            else if (oFinfer.Type == Finger.FingerType.TYPE_RING)
            {
                this.TipRing.UpdateFingerState(vecterFingerTip, vecterPalm, oFinfer);
            }
            else if (oFinfer.Type == Finger.FingerType.TYPE_PINKY)
            {
                this.TipPinky.UpdateFingerState(vecterFingerTip, vecterPalm, oFinfer);
            }
        }

        public void RevertFingerState()
        {
            for (int i = 0; i < this.GdTips.Children.Count; i++)
            {
                TipItem oItem = this.GdTips.Children[i] as TipItem;
                if (oItem != null)
                {
                    oItem.RevertState();
                }
            }
        }
    }
}

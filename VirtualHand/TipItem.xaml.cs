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

namespace VirtualHand
{
    /// <summary>
    /// TipItem.xaml 的交互逻辑
    /// </summary>
    public partial class TipItem : UserControl
    {
        private const double DefaultScale = 0.5;
        public double ScaleX
        {
            get { return (double)GetValue(ScaleXProperty); }
            set { SetValue(ScaleXProperty, value); }
        }
        public static readonly DependencyProperty ScaleXProperty =
            DependencyProperty.Register("ScaleX", typeof(double), typeof(TipItem), new UIPropertyMetadata(1.0));

        public double ScaleY
        {
            get { return (double)GetValue(ScaleYProperty); }
            set { SetValue(ScaleYProperty, value); }
        }
        public static readonly DependencyProperty ScaleYProperty =
            DependencyProperty.Register("ScaleY", typeof(double), typeof(TipItem), new UIPropertyMetadata(1.0));


        public TipItem()
        {
            InitializeComponent();
            this.Loaded += TipItem_Loaded;
            this.DataContext = this;
        }

        private void TipItem_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= TipItem_Loaded;
            RevertState();
        }

        private double Max = 0;
        private double Min = 0;
        private double Sensitive = 0.2;

        private double GetVecterDistance(Leap.Vector vecter1, Leap.Vector vecter2)
        {
            return Math.Sqrt((vecter1.x - vecter2.x) * (vecter1.x - vecter2.x) +
                (vecter1.y - vecter2.y) * (vecter1.y - vecter2.y)+ 
                (vecter1.z - vecter2.z) * (vecter1.z - vecter2.z));
        }

        public void RevertState()
        {
            this.Max = 0;
            this.Sensitive = this.Sensitive * (this.Width / 30);
        }

        //public void UpdateFingerState(Leap.Vector vecterFingerTip, Leap.Vector vecterPalm, Leap.Finger oFinger)
        //{
        //    double dInterval = this.GetVecterDistance(vecterFingerTip, vecterPalm);
        //    if (Max == 0)
        //    {
        //        Max = dInterval;
        //        return;
        //    }
        //    if (dInterval > Max)
        //    {
        //        Max = dInterval;
        //    }

        //    if (dInterval < Sensitive)
        //    {
        //        Sensitive = dInterval;
        //    }

        //    if (dInterval < Sensitive)
        //        dInterval = Sensitive;
        //    double dPercent = (dInterval - Sensitive) / (this.Max - Sensitive);

        //    double dScale = 1 - 0.5 * dPercent;
        //    this.ScaleX = dScale;
        //    this.ScaleY = dScale;
        //}

        public void UpdateFingerState(Leap.Vector vecterFingerTip, Leap.Vector vecterPalm,
            Leap.Finger oFinger)
        {
            if (oFinger.Type != Leap.Finger.FingerType.TYPE_THUMB)
            {
                double angle = (180 - (oFinger.Hand.Direction.AngleTo(oFinger.Direction) / Math.PI) * 180);
                if (this.Max == 0)
                {
                    this.Max = angle;
                    return;
                }

                if (angle > this.Max)
                    this.Max = angle;

                if (angle < this.Min)
                    this.Min = angle;

                double dAngleDeviation = 30d * (1 - angle / 180d);
                double dPercent = (angle - dAngleDeviation - this.Min) / (this.Max - this.Min);
                double dScale = 1 - 0.5 * dPercent;
                this.ScaleX = dScale;
                this.ScaleY = dScale;
            }
            else if (oFinger.Type == Leap.Finger.FingerType.TYPE_THUMB)
            {
                Leap.Bone bone1 = oFinger.Bone(Leap.Bone.BoneType.TYPE_DISTAL);
                Leap.Bone bone2 = oFinger.Bone(Leap.Bone.BoneType.TYPE_PROXIMAL);
                double angle = (180 - (bone1.Direction.AngleTo(bone2.Direction) / Math.PI) * 180);
                if (this.Max == 0)
                {
                    this.Max = angle;
                    return;
                }

                if (angle > this.Max)
                    this.Max = angle;

                if (angle < this.Min)
                    this.Min = angle;

                double dAngleDeviation = (this.Max - this.Min) * (1 - angle / 180d);
                double dPercent = (angle - dAngleDeviation - this.Min) / (this.Max - this.Min);
                double dScale = 1 - 0.5 * dPercent;
                this.ScaleX = dScale;
                this.ScaleY = dScale;
            }
        }
    }
}

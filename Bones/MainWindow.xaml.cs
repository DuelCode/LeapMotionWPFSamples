using Leap;
using System;
using System.Collections;
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

namespace Bones
{
    //public class BoneLine
    //{

    //}

    public partial class MainWindow : Window
    {
        Controller leap = new Controller();
        double CanvasWidth = 1400;
        double CanvasHeight = 800;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= MainWindow_Loaded;
            this.CanvasWidth = this.CvMain.ActualWidth;
            this.CanvasHeight = this.CvMain.ActualHeight;
            CompositionTarget.Rendering += Update;
        }

        protected void Update(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                Pen oPen = new Pen() { Brush = new SolidColorBrush(Colors.Blue), Thickness = 3 };

                Leap.Frame frame = leap.Frame();
                InteractionBox interactionBox = frame.InteractionBox;
                foreach (Hand hand in frame.Hands)
                {
                    foreach (Finger finger in hand.Fingers)
                    {
                        Bone bone;
                        foreach (Bone.BoneType boneType in (Bone.BoneType[])Enum.GetValues(typeof(Bone.BoneType)))
                        {
                            bone = finger.Bone(boneType);
                            Leap.Vector startPosition = interactionBox.NormalizePoint(bone.PrevJoint);
                            Leap.Vector endPosition = interactionBox.NormalizePoint(bone.NextJoint);

                            double dX1 = startPosition.x * this.CanvasWidth;
                            double dY1 = this.CanvasHeight - startPosition.y * this.CanvasHeight;
                            double dX2 = endPosition.x * this.CanvasWidth;
                            double dY2 = this.CanvasHeight - endPosition.y * this.CanvasHeight;

                            drawingContext.DrawLine(oPen, new Point(dX1, dY1), new Point(dX2, dY2));

                            if (boneType == Bone.BoneType.TYPE_PROXIMAL)
                                drawingContext.DrawEllipse(new SolidColorBrush(Colors.Gold), oPen, new Point(dX1, dY1), 6, 6);
                            else if (boneType == Bone.BoneType.TYPE_METACARPAL)
                                drawingContext.DrawEllipse(new SolidColorBrush(Colors.Black), oPen, new Point(dX1, dY1), 6, 6);
                            else if (boneType == Bone.BoneType.TYPE_INTERMEDIATE)
                                drawingContext.DrawEllipse(new SolidColorBrush(Colors.DarkGreen), oPen, new Point(dX1, dY1), 6, 6);
                            else if (boneType == Bone.BoneType.TYPE_DISTAL)
                            {
                                drawingContext.DrawEllipse(new SolidColorBrush(Colors.Red), oPen, new Point(dX1, dY1), 6, 6);
                                // Finger Tip
                                drawingContext.DrawEllipse(new SolidColorBrush(Colors.Red), oPen, new Point(dX2, dY2), 12, 12);
                            }
                        }
                    }
                }

                drawingContext.Close();

                RenderTargetBitmap rtb = new RenderTargetBitmap((int)CvMain.ActualWidth,
                    (int)this.CvMain.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(drawingVisual);

                this.ImgMain.Source = rtb;
            }));
        }
    }
}

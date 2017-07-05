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

namespace VirtualHand
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Controller leap = new Controller();
        double CanvasWidth = 1360;
        double CanvasHeight = 768;
        private Palm PalmPlane;

        public MainWindow()
        {
            InitializeComponent();

            leap.SetPolicy(Controller.PolicyFlag.POLICY_IMAGES);

            this.PalmPlane = new Palm();
            this.CvMain.Children.Add(this.PalmPlane);

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= MainWindow_Loaded;
            this.CanvasWidth = this.CvMain.ActualWidth;
            this.CanvasHeight = this.CvMain.ActualHeight;
            CompositionTarget.Rendering += Update;
        }

        private SolidColorBrush BrushRed = new SolidColorBrush(Colors.Red);
        private SolidColorBrush BrushBlue = new SolidColorBrush(Colors.Blue);

        private Point ToScreen(Leap.Vector vector)
        {
            double dX = vector.x * this.CanvasWidth;
            double dY = this.CanvasHeight - vector.y * this.CanvasHeight;
            return new Point(dX, dY);
        }

        private void DrawFingerBones(Leap.Finger oFinger, InteractionBox interactionBox,
            DrawingContext drawingContext)
        {
            Bone bone;
            foreach (Bone.BoneType boneType in (Bone.BoneType[])Enum.GetValues(typeof(Bone.BoneType)))
            {
                bone = oFinger.Bone(boneType);
                Leap.Vector startPosition = interactionBox.NormalizePoint(bone.PrevJoint);
                Leap.Vector endPosition = interactionBox.NormalizePoint(bone.NextJoint);

                Point posStart = this.ToScreen(startPosition);
                Point posEnd = this.ToScreen(endPosition);
                double dX1 = posStart.X;
                double dY1 = posStart.Y;
                double dX2 = posEnd.X;
                double dY2 = posEnd.Y;
                drawingContext.DrawLine(new Pen() { Brush = new SolidColorBrush(Colors.Blue), Thickness = 1 },
                    new Point(dX1, dY1), new Point(dX2, dY2));

                Pen oPen = new Pen();
                if (boneType == Bone.BoneType.TYPE_PROXIMAL)
                    drawingContext.DrawEllipse(new SolidColorBrush(Colors.Gold), oPen, new Point(dX1, dY1), 6, 6);
                else if (boneType == Bone.BoneType.TYPE_METACARPAL)
                    drawingContext.DrawEllipse(new SolidColorBrush(Colors.Black), oPen, new Point(dX1, dY1), 6, 6);
                else if (boneType == Bone.BoneType.TYPE_INTERMEDIATE)
                    drawingContext.DrawEllipse(new SolidColorBrush(Colors.DarkGreen), oPen, new Point(dX1, dY1), 6, 6);
                else if (boneType == Bone.BoneType.TYPE_DISTAL)
                {
                    drawingContext.DrawEllipse(new SolidColorBrush(Colors.Red), oPen, new Point(dX1, dY1), 6, 6);
                    drawingContext.DrawEllipse(new SolidColorBrush(Colors.Red), oPen, new Point(dX2, dY2), 12, 12);
                }
            }
        }

        WriteableBitmap bitmap;
        private void DrawRawImages_WriteableBitmap(byte[] rawImageData, int dWidth, int dHeight)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                if (bitmap == null || ImgCamera.Source == null)
                {
                    List<System.Windows.Media.Color> grayscale = new List<System.Windows.Media.Color>();
                    for (byte i = 0; i < 0xff; i++)
                    {
                        grayscale.Add(System.Windows.Media.Color.FromArgb(0xff, i, i, i));
                    }
                    BitmapPalette palette = new BitmapPalette(grayscale);
                    bitmap = new WriteableBitmap(dWidth, dHeight, 72, 72, PixelFormats.Gray8, palette);
                    ImgCamera.Source = bitmap;
                }
                bitmap.WritePixels(new Int32Rect(0, 0, dWidth, dHeight), rawImageData, dWidth * bitmap.Format.BitsPerPixel / 8, 0);
            }));
        }

        protected void Update(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                Leap.Frame frame = leap.Frame();

                //Draw Camera Images
                try
                {
                    Leap.Image image = frame.Images[0];
                    this.DrawRawImages_WriteableBitmap(image.Data, image.Width, image.Height);
                }
                catch { }
                

                if (!frame.Hands.IsEmpty)
                {
                    DrawingVisual drawingVisual = new DrawingVisual();
                    DrawingContext drawingContext = drawingVisual.RenderOpen();

                    InteractionBox interactionBox = frame.InteractionBox;
                    
                    // Draw Hand Center
                    Hand oHand = frame.Hands.FirstOrDefault();
                    Leap.Vector handNormalizePos = interactionBox.NormalizePoint(oHand.StabilizedPalmPosition);
                    Point handPos = this.ToScreen(handNormalizePos);
                    drawingContext.DrawEllipse(this.BrushBlue, new Pen(), new Point(handPos.X, handPos.Y), 20, 20);
                    
                    // Draw finger tip and update Palm State
                    foreach (Finger oFinger in oHand.Fingers)
                    {
                        // Draw Bones
                        this.DrawFingerBones(oFinger, interactionBox, drawingContext);

                        Leap.Vector fingerNormalizePos = interactionBox.NormalizePoint(oFinger.StabilizedTipPosition);
                        //Point fingerPos = this.ToScreen(fingerNormalizePos);
                        //drawingContext.DrawEllipse(this.BrushRed, new Pen(), new Point(fingerPos.X, fingerPos.Y), 6, 6);
                        this.PalmPlane.UpdatePalmState(oFinger, fingerNormalizePos, handNormalizePos);
                    }

                    // Update Palm Location 
                    this.PalmPlane.X = handPos.X - this.PalmPlane.Width / 2d;
                    this.PalmPlane.Y = handPos.Y - this.PalmPlane.Height / 2d;

                    drawingContext.Close();
                    RenderTargetBitmap rtb = new RenderTargetBitmap((int)CvMain.ActualWidth,
                        (int)this.CvMain.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                    rtb.Render(drawingVisual);

                    this.ImgMain.Source = rtb;
                }
                else
                {
                    if (this.PalmPlane != null)
                    {
                        this.PalmPlane.X = -300;
                        this.PalmPlane.Y = -300;
                        this.PalmPlane.RevertFingerState();
                    }
                    this.ImgMain.Source = null;
                }
            }));
        }
    }
}

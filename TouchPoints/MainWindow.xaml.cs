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
using System.Windows.Ink;

namespace TouchPoints
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Controller leap = new Controller();
        float windowWidth = 1400;
        float windowHeight = 800;
        DrawingAttributes touchIndicator = new DrawingAttributes();

        public MainWindow()
        {
            InitializeComponent();
            leap.SetPolicy(Controller.PolicyFlag.POLICY_IMAGES);

            CompositionTarget.Rendering += Update;
            touchIndicator.Width = 20;
            touchIndicator.Height = 20;
            touchIndicator.StylusTip = StylusTip.Ellipse;
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
            paintCanvas.Strokes.Clear();

            windowWidth = (float)this.Width;
            windowHeight = (float)this.Height;

            Leap.Frame frame = leap.Frame();
            InteractionBox interactionBox = leap.Frame().InteractionBox;
            try
            {
                Leap.Image image = frame.Images[0];
                this.DrawRawImages_WriteableBitmap(image.Data, image.Width, image.Height);
            }
            catch { }

            foreach (Pointable pointable in leap.Frame().Pointables)
            {
                Finger oFinger = new Finger(pointable);
                Leap.Vector normalizedPosition =
                    interactionBox.NormalizePoint(pointable.StabilizedTipPosition);
                float tx = normalizedPosition.x * windowWidth;
                float ty = windowHeight - normalizedPosition.y * windowHeight;

                int alpha = 255;
                if (pointable.TouchDistance > 0 && pointable.TouchZone != Pointable.Zone.ZONENONE)
                {
                    alpha = 255 - (int)(255 * pointable.TouchDistance);
                    touchIndicator.Color = Color.FromArgb((byte)alpha, 0x0, 0xff, 0x0);
                }
                else if (pointable.TouchDistance <= 0)
                {
                    alpha = -(int)(255 * pointable.TouchDistance);
                    touchIndicator.Color = Color.FromArgb((byte)alpha, 0xff, 0x0, 0x0);
                }
                else
                {
                    alpha = 50;
                    touchIndicator.Color = Color.FromArgb((byte)alpha, 0x0, 0x0, 0xff);
                }
                StylusPoint touchPoint = new StylusPoint(tx, ty);
                StylusPointCollection tips =
                    new StylusPointCollection(new StylusPoint[] { touchPoint });
                Stroke touchStroke = new Stroke(tips, touchIndicator);
                paintCanvas.Strokes.Add(touchStroke);
            }
        }
    }
}

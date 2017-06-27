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
using System.Windows.Ink;

namespace SampleOne
{
    public interface ILeapEventDelegate
    {
        void LeapEventNotification(string EventName);
    }

    public class LeapEventListener : Listener
    {
        ILeapEventDelegate eventDelegate;

        public LeapEventListener(ILeapEventDelegate delegateObject)
        {
            this.eventDelegate = delegateObject;
        }

        public override void OnInit(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onInit");
        }

        public override void OnConnect(Controller controller)
        {
            controller.SetPolicy(Controller.PolicyFlag.POLICY_IMAGES);
            controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
            this.eventDelegate.LeapEventNotification("onConnect");
        }

        public override void OnFrame(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onFrame");
        }
        public override void OnExit(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onExit");
        }
        public override void OnDisconnect(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onDisconnect");
        }
    }

    public partial class MainWindow : Window, ILeapEventDelegate
    {
        private Controller LeapController = new Controller();
        private LeapEventListener LeapListener;
        delegate void LeapEventDelegate(string EventName);
        private bool IsClosing = false; 

        public MainWindow()
        {
            InitializeComponent();
            this.InitLeapListener();
            this.InitInkCanvasPen();
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Closing -= MainWindow_Closing;
            App.Current.Shutdown();

            //this.IsClosing = true;
            //this.LeapController.RemoveListener(this.LeapListener); 
            //this.LeapController.Dispose();
        }

        private void InitLeapListener()
        {
            this.LeapController = new Controller();
            this.LeapListener = new LeapEventListener(this);
            LeapController.AddListener(LeapListener);
        }

        private void InitInkCanvasPen()
        {
            touchIndicator.Width = 20;
            touchIndicator.Height = 20;
            touchIndicator.StylusTip = StylusTip.Ellipse;
        }

        // Implement Interface
        public void LeapEventNotification(string EventName)
        {
            if (this.CheckAccess())
            {
                switch (EventName)
                {
                    case "onInit":
                        break;
                    case "onConnect":
                        this.ConnectHandler();
                        break;
                    case "onFrame":
                        if (!this.IsClosing)
                            this.NewFrameHandler(this.LeapController.Frame());
                        break;
                }
            }
            else
            {
                Dispatcher.Invoke(new LeapEventDelegate(LeapEventNotification), new object[] { EventName });
            }
        }

        // Set Controller Configs
        private void ConnectHandler()
        {
            this.LeapController.SetPolicy(Controller.PolicyFlag.POLICY_IMAGES);
            this.LeapController.Config.SetFloat("Gesture.Swipe.MinLength", 100.0f);

            this.LeapController.EnableGesture(Gesture.GestureType.TYPE_CIRCLE);
            this.LeapController.EnableGesture(Gesture.GestureType.TYPE_KEY_TAP);
            this.LeapController.EnableGesture(Gesture.GestureType.TYPE_SCREEN_TAP);
            this.LeapController.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
        }

        // Render
        private void NewFrameHandler(Leap.Frame frame)
        {
            try
            {
                this.ShowRawImages(frame);
                this.ShowGeneralInfo(frame);
                this.ShowFingerPoints(frame);
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


        // 最新采集图像
        WriteableBitmap CurBitmap;

        // 绘制采集到的图像
        private void ShowRawImages(Leap.Frame frame)
        {
            Leap.Image image = frame.Images[0];
            byte[] rawImageData = image.Data;
            int dWidth = image.Width;
            int dHeight = image.Height;

            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                if (this.CurBitmap == null || this.ImgMain.Source == null)
                {
                    List<System.Windows.Media.Color> grayscale = new List<System.Windows.Media.Color>();
                    for (byte i = 0; i < 0xff; i++)
                    {
                        grayscale.Add(System.Windows.Media.Color.FromArgb(0xff, i, i, i));
                    }
                    BitmapPalette palette = new BitmapPalette(grayscale);
                    this.CurBitmap = new WriteableBitmap(dWidth, dHeight, 72, 72, PixelFormats.Gray8, palette);
                    this.ImgMain.Source = this.CurBitmap;
                }
                this.CurBitmap.WritePixels(new Int32Rect(0, 0, dWidth, dHeight), rawImageData,
                    dWidth * this.CurBitmap.Format.BitsPerPixel / 8, 0);
            }));
        }

        // 实时采集信息显示
        private void ShowGeneralInfo(Leap.Frame frame)
        {
            this.TbkFrameId.Text = "Frame ID: " + frame.Id;
            this.TbkTimestamp.Text = "Timestamp: " + frame.Timestamp;
            this.TbkGesturesCount.Text = "Gestures Count: " + frame.Gestures().Count;
            this.TbkHandsCount.Text = "Hands Count: " + frame.Hands.Count;
            this.TbkFingersCount.Text = "Fingers Count: " + frame.Fingers.Count;
            this.TbkToolsCount.Text = "Tools Count: " + frame.Tools.Count;
        }

        DrawingAttributes touchIndicator = new DrawingAttributes();
        private void ShowFingerPoints(Leap.Frame frame)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                this.InkCanvasPnl.Strokes.Clear();

                InteractionBox interactionBox = frame.InteractionBox;

                foreach (Pointable pointable in frame.Pointables)
                {
                    Leap.Vector normalizedPosition =
                        interactionBox.NormalizePoint(pointable.StabilizedTipPosition);
                    double tx = normalizedPosition.x * this.InkCanvasPnl.Width;
                    double ty = this.InkCanvasPnl.Height - normalizedPosition.y * this.InkCanvasPnl.Height;

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
                    this.InkCanvasPnl.Strokes.Add(touchStroke);
                }
            }));
        }
    }
}

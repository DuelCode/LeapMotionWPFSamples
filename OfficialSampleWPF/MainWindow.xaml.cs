using Leap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OfficialSampleWPF
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
        private Controller controller = new Controller();
        private LeapEventListener listener;
        private bool isClosing = false;
        WriteableBitmap bitmap;

        delegate void LeapEventDelegate(string EventName);

        public MainWindow()
        {
            InitializeComponent();

            this.controller = new Controller();
            this.listener = new LeapEventListener(this);
            controller.AddListener(listener);
        }


        public void LeapEventNotification(string EventName)
        {
            if (this.CheckAccess())
            {
                switch (EventName)
                {
                    case "onInit":
                        this.debugText.Text += "Init " + "\r\n";
                        break;
                    case "onConnect":
                        this.connectHandler();
                        break;
                    case "onFrame":
                        if (!this.isClosing)
                            this.newFrameHandler(this.controller.Frame());
                        break;
                }
            }
            else
            {
                Dispatcher.Invoke(new LeapEventDelegate(LeapEventNotification), new object[] { EventName });
            }
        }

        void connectHandler()
        {
            this.controller.SetPolicy(Controller.PolicyFlag.POLICY_IMAGES);
            this.controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
            this.controller.Config.SetFloat("Gesture.Swipe.MinLength", 100.0f);
        }

        private void DrawRawImages_Bitmap(byte[] rawImageData, int dWidth, int dHeight)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                System.Drawing.Bitmap newFrame = new System.Drawing.Bitmap(dWidth, dHeight,
                    System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                ColorPalette grayscale = newFrame.Palette;
                for (int i = 0; i < 256; i++)
                {
                    grayscale.Entries[i] = System.Drawing.Color.FromArgb((int)255, i, i, i);
                }
                newFrame.Palette = grayscale;
                System.Drawing.Rectangle lockArea = new System.Drawing.Rectangle(0, 0, newFrame.Width, newFrame.Height);
                BitmapData bitmapData = newFrame.LockBits(lockArea, ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

                System.Runtime.InteropServices.Marshal.Copy(rawImageData, 0, bitmapData.Scan0, dWidth * dHeight);

                var bitmapSource = BitmapSource.Create(bitmapData.Width, bitmapData.Height, 96, 96, PixelFormats.Gray8, null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                newFrame.UnlockBits(bitmapData);

                this.displayImages.Source = bitmapSource;
            }));
        }

        private void DrawRawImages_WriteableBitmap(byte[] rawImageData, int dWidth, int dHeight)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                if (bitmap == null || displayImages.Source == null)
                {
                    List<System.Windows.Media.Color> grayscale = new List<System.Windows.Media.Color>();
                    for (byte i = 0; i < 0xff; i++)
                    {
                        grayscale.Add(System.Windows.Media.Color.FromArgb(0xff, i, i, i));
                    }
                    BitmapPalette palette = new BitmapPalette(grayscale);
                    bitmap = new WriteableBitmap(dWidth, dHeight, 72, 72, PixelFormats.Gray8, palette);
                    displayImages.Source = bitmap;
                }
                bitmap.WritePixels(new Int32Rect(0, 0, dWidth, dHeight), rawImageData, dWidth * bitmap.Format.BitsPerPixel / 8, 0);
            }));
        }

        void newFrameHandler(Leap.Frame frame)
        {
            try
            {
                this.displayID.Content = frame.Id.ToString();
                this.displayTimestamp.Content = frame.Timestamp.ToString();
                this.displayFPS.Content = frame.CurrentFramesPerSecond.ToString();
                this.displayIsValid.Content = frame.IsValid.ToString();
                this.displayGestureCount.Content = frame.Gestures().Count.ToString();
                this.displayImageCount.Content = frame.Images.Count.ToString();

                // Bitmap To BitmapSource方式获取图像
                //Leap.Image image = frame.Images[0];
                //this.DrawRawImages_Bitmap(image.Data, image.Width, image.Height);

                //WriteableBitmap方式
                Leap.Image image = frame.Images[0];
                this.DrawRawImages_WriteableBitmap(image.Data, image.Width, image.Height);
            }
            catch (Exception ex)
            {
                this.debugText.Text += "Init " + "\r\n";
            }
        }

        public BitmapSource ConvertBitmapToBiamapSource(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height, 96, 96, PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try {
                App.Current.Shutdown();
                return;
                // Todo: 执行 this.controller.RemoveListener(this.listener); 时程序会处于“无响应”状态，
                // 尚不清楚原因。 所以直接结束进程.

                this.isClosing = true;
                this.controller.RemoveListener(this.listener);
                this.controller.Dispose();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }
    }

   
}

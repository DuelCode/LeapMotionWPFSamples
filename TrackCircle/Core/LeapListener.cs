using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;
using System.Diagnostics;

namespace TrackCircle.Core
{
    class LimitQueue<T> : Queue<T>
    {
        public int Max { get; set; }

        public LimitQueue(int max)
        {
            Max = max;
        }

        public new void Enqueue(T item)
        {
            base.Enqueue(item);

            while (Count > Max)
            {
                Dequeue();
            }
        }
    }

    public class LeapListener : Listener
    {
        private Object thisLock = new Object();

        private void SafeWriteLine(String line)
        {
            lock (thisLock)
            {
                Console.WriteLine(line);
                Debug.WriteLine(line);
            }
        }

        public override void OnInit(Controller controller)
        {
            SafeWriteLine("Initialized");
        }

        public override void OnConnect(Controller controller)
        {
            SafeWriteLine("Connected");

        }

        public override void OnDisconnect(Controller controller)
        {
            SafeWriteLine("Disconnected");
        }

        public override void OnExit(Controller controller)
        {
            SafeWriteLine("Exited");
        }

        private long _now;
        private long _previous;
        private long _timeDifference;

        public override void OnFrame(Controller controller)
        {
            Leap.Frame frame = controller.Frame();
            if (!frame.Hands.IsEmpty)
            {
                var screen = controller.LocatedScreens.FirstOrDefault();
                if (screen != null && screen.IsValid)
                {
                    if (frame.Fingers.Count > 0)
                    {
                        _now = frame.Timestamp;
                        _timeDifference = _now - _previous;
                        _previous = frame.Timestamp;
                        if (_timeDifference >= 1000)
                        {
                            Finger oFinger = frame.Fingers[0];
                            var coordinate = GetNormalizedXAndY(oFinger, screen);

                            if (this.OnFingerLocationChanged != null)
                                Task.Factory.StartNew(() => OnFingerLocationChanged(coordinate.X, coordinate.Y));
                        }
                    }

                    var pos = FindCursorPosition(frame.Hands[0], screen);
                    if (this.OnHandLocationChanged != null)
                        Task.Factory.StartNew(() => OnHandLocationChanged(pos.x, pos.y));
                }
            }
        }

        #region hand

        private LimitQueue<Vector> positionAverage = new LimitQueue<Vector>(30);
        private LimitQueue<Vector> velocityAverage = new LimitQueue<Vector>(10);
        private Vector lastMousePos = Vector.Zero;

        private Vector FindCursorPosition(Hand hand, Screen screen)
        {
            var position = PalmDirectionMethod(hand, screen);
            var diff = position - lastMousePos;
            lastMousePos = position;

            velocityAverage.Enqueue(diff);

            if (Average(velocityAverage).Magnitude > 0.5 || !positionAverage.Any())
            {
                positionAverage.Enqueue(position);
            }

            return Average(positionAverage);
        }

        private Vector PalmDirectionMethod(Hand hand, Screen screen)
        {
            var handScreenIntersection = VectorPlaneIntersection(hand.PalmPosition, hand.Direction, 
                screen.BottomLeftCorner, screen.Normal());

            var screenPoint = handScreenIntersection - screen.BottomLeftCorner;
            var screenPointHorizontal = screenPoint.Dot(screen.HorizontalAxis.Normalized) / screen.HorizontalAxis.Magnitude;
            var screenPointVertical = screenPoint.Dot(screen.VerticalAxis.Normalized) / screen.VerticalAxis.Magnitude;
            var screenRatios = new Vector(screenPointHorizontal, screenPointVertical, 0);

            screenRatios.x = (screenRatios.x - 0.5f) * 1.7f + 0.3f; // increase X sensitivity by 70% and shift left 20%
            screenRatios.y = (screenRatios.y - 0.5f) * 1.5f + 0.5f; // increase X sensitivity by 50%
            var screenCoords = ToScreen(screenRatios, screen);
            return screenCoords;
        }

        private Vector VectorPlaneIntersection(Vector vectorPoint, Vector vectorDirection, Vector planePoint, Vector planeNormal)
        {
            float distance = ((planePoint - vectorPoint).Dot(planeNormal) / (vectorDirection.Dot(planeNormal)));

            return vectorPoint + vectorDirection * distance;
        }

        private Vector Average(IEnumerable<Vector> vectors)
        {
            return new Vector(vectors.Average(v => v.x), vectors.Average(v => v.y), vectors.Average(v => v.z));
        }

        private Vector ToScreen(Vector v, Screen s)
        {
            var screenX = (int)Math.Min(s.WidthPixels, Math.Max(0, (v.x * s.WidthPixels)));
            var screenY = (int)Math.Min(s.HeightPixels, Math.Max(0, (s.HeightPixels - v.y * s.HeightPixels)));

            return new Vector(screenX, screenY, 0);
        }

        #endregion


        #region finger

        private System.Windows.Point GetNormalizedXAndY(Finger oFinger, Screen screen)
        {
            var xNormalized = screen.Intersect(oFinger, true, 1.0F).x;
            var yNormalized = screen.Intersect(oFinger, true, 1.0F).y;

            var x = (xNormalized * screen.WidthPixels);
            var y = screen.HeightPixels - (yNormalized * screen.HeightPixels);

            return new System.Windows.Point() { X = x, Y = y };
        }

        #endregion


        #region Events

        public delegate void LocationChangedEventHander(double dX, double dY);

        public event LocationChangedEventHander OnFingerLocationChanged;

        public event LocationChangedEventHander OnHandLocationChanged;

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;   
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace DrawingRobot
{
    public class Arm
    {
        public double length1, length2;
        public Servo baseServo, elbowServo;

        public event EventHandler Move; 
        private CancellationTokenSource source;

        //UI Variables
        private Line shoulderToElbowLine;
        private Line elbowToHandLine;

        public Canvas accuracyCanvas, boundariesCanvas, armCanvas, debugCanvas;

        private List<UIElement> elements = new List<UIElement>();

        public Arm(double length1, double length2, Servo baseServo, Servo elbowServo)
        {
            this.length1 = length1;
            this.length2 = length2;
            this.baseServo = baseServo;
            this.elbowServo = elbowServo;

            CreateUI();

            baseServo.Move += OnMove;
            elbowServo.Move += OnMove;
        }

        private void CreateUI()
        {
            accuracyCanvas = new Canvas();
            boundariesCanvas = new Canvas();
            debugCanvas = new Canvas();

            armCanvas = new Canvas();

            shoulderToElbowLine = new Line()
            {
                Stroke = Brushes.Red,
                StrokeThickness = 8.0,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            elbowToHandLine = new Line()
            {
                Stroke = Brushes.Red,
                StrokeThickness = 8.0,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Triangle
            };

            armCanvas.Children.Add(shoulderToElbowLine);
            armCanvas.Children.Add(elbowToHandLine);

            UpdateAccuracyCanvas();
            UpdateBoundaryCanvas();
            UpdateArmCanvas();
        }

        public void OnMove(object sender, EventArgs e)
        {
            Move?.Invoke(sender, e);
        }

        public double GetReach(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        public double GetMaxReach()
        {
            return length1 + length2;
        }

        public double GetMinReach()
        {
            return Math.Max(length1 - length2, 0);
        }

        public bool InRange(double x, double y)
        {

            double minReach = GetMinReach();
            double maxReach = GetMaxReach();

            double reach = GetReach(x, y);

            if (reach > maxReach)
                return false;
            else if (reach < minReach)
                return false;

            double[] newServoRot = CalculateServoRotations(x, y);

            if (!baseServo.InRange(newServoRot[0]))
                return false;
            else if (!elbowServo.InRange(newServoRot[1]))
                return false;

            return true;
        }

        public double[] CalculateServoRotations(double x, double y)
        {
            double c = length1;
            double a = length2;
            double e = x;
            double d = y;

            double G = Math.Atan2(d, e);
            double b = Math.Sqrt(e * e + d * d);
            double B = Math.Acos((a * a + c * c - b * b) / (2 * a * c));
            double A = Math.Acos((b * b + c * c - a * a) / (2 * b * c));

            double theta1 = G + A;
            double theta2 = B;

            return new double[2] { theta1, theta2 };
        }

        public double[] GetHandPosition()
        {
            double c = length1;
            double a = length2;

            double theta1 = baseServo.angle;
            double B = elbowServo.angle;

            double w1 = c * Math.Cos(theta1);
            double h1 = c * Math.Sin(theta1);

            double thetah = B + theta1 - Math.PI;

            double w2 = a * Math.Cos(thetah);
            double h2 = a * Math.Sin(thetah);

            double w = w1 + w2;
            double h = h1 + h2;

            return new double[2] { w, h };
        }

        public double[] GetHandPosition(double baseAngle, double elbowAngle)
        {
            double c = length1;
            double a = length2;

            double theta1 = baseAngle;
            double B = elbowAngle;

            double w1 = c * Math.Cos(theta1);
            double h1 = c * Math.Sin(theta1);

            double thetah = B + theta1 - Math.PI;

            double w2 = a * Math.Cos(thetah);
            double h2 = a * Math.Sin(thetah);

            double w = w1 + w2;
            double h = h1 + h2;

            return new double[2] { w, h };
        }

        public async Task<bool> SetPosition(double x, double y)
        {
            if (source != null)
                source.Cancel();
           
            source = new CancellationTokenSource();

            double[] servoAngles = CalculateServoRotations(x, y);

            bool rotationSuccessful = false;

            try
            {
                Task<bool> rotateBaseServo = baseServo.RotateTo(servoAngles[0], source.Token);
                Task<bool> rotateElbowServo = elbowServo.RotateTo(servoAngles[1], source.Token);

                // Start rotating the servos to the new positions and get the one that finishes first
                Task<bool> completedTask = await Task.WhenAny(rotateBaseServo, rotateElbowServo);

                // Check if the servo was successful at rotating. If it wasn't, then there is no reason to continue
                if (completedTask.Result)
                {
                    // Wait for the other servo to finish
                    await Task.WhenAll(rotateBaseServo, rotateElbowServo);

                    // Check that both servos were successful
                    if (rotateBaseServo.Result && rotateElbowServo.Result)
                        rotationSuccessful = true;
                }
            }
            catch (AggregateException e)
            {
                rotationSuccessful = false;
            }
            catch (OperationCanceledException e)
            {
                rotationSuccessful = false;
            }
            
            // Return whether both of the servos were succesfully rotated to their new positions
            return rotationSuccessful;
        }

        public void UpdateArmCanvas()
        {
            double elbowX = Math.Cos(baseServo.angle) * length1;
            double elbowY = Math.Sin(baseServo.angle) * length1;

            double[] handPosition = GetHandPosition();

            double handX = handPosition[0];
            double handY = handPosition[1];

            shoulderToElbowLine.X1 = 0;
            shoulderToElbowLine.Y1 = 0;

            shoulderToElbowLine.X2 = elbowX;
            shoulderToElbowLine.Y2 = elbowY;

            elbowToHandLine.X1 = elbowX;
            elbowToHandLine.Y1 = elbowY;

            elbowToHandLine.X2 = handX;
            elbowToHandLine.Y2 = handY;
        }

        public void UpdateAccuracyCanvas()
        {
            accuracyCanvas.Children.Clear();

            System.Diagnostics.Debug.Print(baseServo.maxAngle.ToString());

            for (double baseServoRot = baseServo.minAngle; baseServoRot < baseServo.maxAngle; baseServoRot += baseServo.accuracy)
            {
                for (double elbowServoRot = elbowServo.minAngle; elbowServoRot < elbowServo.maxAngle; elbowServoRot += elbowServo.accuracy)
                {
                    System.Diagnostics.Debug.Print("" + baseServoRot + " - " + elbowServoRot);
                    double[] pos = GetHandPosition(baseServoRot, elbowServoRot);

                    Ellipse ellipse = new Ellipse()
                    {
                        Fill = Brushes.Blue,
                        Width = 2,
                        Height = 2,
                        Margin = new Thickness(pos[0], pos[1], 0, 0)
                    };

                    accuracyCanvas.Children.Add(ellipse);
                }
            }
        }

        public void UpdateBoundaryCanvas()
        {
            boundariesCanvas.Children.Clear();

            DoubleCollection dc = new DoubleCollection();
            dc.Add(5.0);

            Polygon polygon = new Polygon()
            {
                Stroke = Brushes.Black,
                Fill = Brushes.WhiteSmoke,
                FillRule = FillRule.EvenOdd,
                StrokeThickness = 2,
                StrokeDashArray = dc
            };

            PointCollection myPointCollection = new PointCollection();

            double angleA = baseServo.minAngle;
            double angleB = elbowServo.minAngle;

            // AngleB min to max
            for (int i = 0; i <= 100; i++)
            {
                double percent = i / 100.0;
                angleB = (elbowServo.maxAngle - elbowServo.minAngle) * percent + elbowServo.minAngle;

                double[] handPos = GetHandPosition(angleA, angleB);
                Point p = new Point(handPos[0], handPos[1]);
                
                myPointCollection.Add(p);
            }

            // AngleA min to max
            for (int i = 0; i <= 100; i++)
            {
                double percent = i / 100.0;
                angleA = (baseServo.maxAngle - baseServo.minAngle) * percent + baseServo.minAngle;

                double[] handPos = GetHandPosition(angleA, angleB);
                Point p = new Point(handPos[0], handPos[1]);

                myPointCollection.Add(p);
            }

            // AngleB max to min
            for (int i = 0; i <= 100; i++)
            {
                double percent = 1.0 - i / 100.0;
                angleB = (elbowServo.maxAngle - elbowServo.minAngle) * percent + elbowServo.minAngle;

                double[] handPos = GetHandPosition(angleA, angleB);
                Point p = new Point(handPos[0], handPos[1]);

                myPointCollection.Add(p);
            }

            // AngleA max to min
            for (int i = 0; i <= 100; i++)
            {
                double percent = 1.0 - i / 100.0;
                angleA = (baseServo.maxAngle - baseServo.minAngle) * percent + baseServo.minAngle;

                double[] handPos = GetHandPosition(angleA, angleB);
                Point p = new Point(handPos[0], handPos[1]);

                myPointCollection.Add(p);
            }

            polygon.Points = myPointCollection;

            boundariesCanvas.Children.Add(polygon);
        }
    }
}

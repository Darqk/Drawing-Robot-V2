using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DrawingRobot
{
    public class Servo
    {
        public double angle;

        public double minAngle;
        public double maxAngle;

        public double speed;
        public double accuracy;

        public event EventHandler Move;

        public Servo(double angle, double minAngle, double maxAngle, double speed, double accuracy)
        {
            this.angle = angle;
            this.minAngle = minAngle;
            this.maxAngle = maxAngle;
            this.speed = speed;
            this.accuracy = accuracy;
        }

        public double Clamp(double angle)
        {
            angle = angle % (2.0 * Math.PI);

            if (angle < 0) angle += 2.0 * Math.PI;

            return angle;
        }

        public bool InRange(double angle)
        {
            angle = Clamp(angle);

            if (angle > maxAngle)
                return false;
            else if (angle < minAngle)
                return false;

            return true;
        }

        public double GetAngleFromPercent(double percent)
        {
            return (maxAngle - minAngle) * percent + minAngle;
        }

        public async Task<bool> RotateTo(double newAngle, CancellationToken cancelToken)
        {
            double startAngle = angle;
            // Determine the degree we need to change
            double offset = newAngle - angle;

            // Determine the time it will take to rotate to the new angle
            int milliseconds = (int)Math.Floor(Math.Abs(offset) / speed * 1000);

            for (int i = 0; i < milliseconds; i+=5)
            {
                double percent = Math.Min(1, (i + 1) / (double)milliseconds);

                if (cancelToken.IsCancellationRequested)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    System.Diagnostics.Debug.Print("Servo cancelled at " + percent + " percent.");
                    return false;
                }

                angle = startAngle + offset * percent;

                Move?.Invoke(this, new EventArgs());

                //await Task.Delay(5);
            }

            angle = newAngle;

            // Return whether the servo was successfully rotated
            return true;
        }
    }
}

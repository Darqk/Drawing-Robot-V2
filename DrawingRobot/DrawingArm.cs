using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DrawingRobot
{
    public class DrawingArm
    {
        private Arm arm;
        private Servo linearServo;

        public bool isLowered = false;

        public DrawingArm(Arm arm, Servo linearServo)
        {
            this.arm = arm;
            this.linearServo = linearServo;
        }

        public bool DrawLine()
        {
            //await LowerArm();

            //Move arm

            //await RaiseArm()

            return true;
        }

        public bool RaiseArm()
        {
            return true;
        }

        public bool LowerArm()
        {
            return true;
        }

        private Rect GetMaxDimensions(double aspectRatio)
        {
            double innerRadius = arm.GetMinReach();
            double outerRadius = arm.GetMaxReach();

            double sqrInnerRadius = innerRadius * innerRadius;
            double sqrOuterRadius = outerRadius * outerRadius;
            double sqrAspectRatio = aspectRatio * aspectRatio;

            double sqrt = Math.Sqrt(outerRadius * outerRadius - (sqrAspectRatio * (sqrInnerRadius - sqrOuterRadius)) / 4.0);

            double height = (2.0 * (-2.0 * innerRadius + 2.0 * sqrt)) / (4.0 + sqrAspectRatio);
            double width = height * aspectRatio;

            // Move the rectangle half the width to the left
            double x = -width / 2.0;
            // Move the the inner radius down so it is within the boundaries of the arm
            double y = innerRadius;

            return new Rect(x, y, width, height);
        }
    }
}

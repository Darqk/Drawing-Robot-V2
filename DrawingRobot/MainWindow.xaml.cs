using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace DrawingRobot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Arm arm;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();

            // double rotation = Math.Asin((r2 - r1) / (r1 + r2));
            //double rotation = Math.Asin((125.0 - 25.0) / (25.0 + 125.0));
        }

        private void Initialize()
        {
            Servo baseServo = new Servo(0, 0, Math.PI, Math.PI, 0.5);
            Servo elbowServo = new Servo(Math.PI / 2.0, 0, Math.PI, Math.PI, 0.5);

            arm = new Arm(150, 125, baseServo, elbowServo);

            CreateUI();

            canvas.Children.Add(arm.boundariesCanvas);
            canvas.Children.Add(arm.accuracyCanvas);
            canvas.Children.Add(arm.armCanvas);
            canvas.Margin = new Thickness(360, 360, 0, 0);


            arm.Move += Arm_Move; ;
            canvas.MouseMove += Canvas_MouseMove; ;
        }

        private void Arm_Move(object sender, EventArgs e)
        {
            arm.UpdateArmCanvas();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(canvas);

            double x = p.X;
            double y = p.Y;

            if (arm.InRange(x, y))
                arm.SetPosition(x, y);
        }

        private void CreateUI()
        {
            Slider baseLengthSlider = CreateSlider("Length of base arm: ", arm.length1, 25, 200);
            baseLengthSlider.ValueChanged += BaseLengthSlider_ValueChanged;

            Slider elbowLengthSlider = CreateSlider("Length of elbow arm: ", arm.length2, 25, 200);
            elbowLengthSlider.ValueChanged += ElbowLengthSlider_ValueChanged;
        }

        private Slider CreateSlider(string varName, double curVal, double minVal, double maxVal)
        {
            AddVarName(varName);

            Slider slider = new Slider()
            {
                Minimum = minVal,
                Maximum = maxVal,
                Value = curVal
            };

            variableValues.Children.Add(slider);

            return slider;
        }

        private void AddVarName(string text)
        {
            TextBlock textBlock = new TextBlock()
            {
                Text = text
            };

            variableNames.Children.Add(textBlock);
        }
       
        private void ElbowLengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            arm.length2 = e.NewValue;
            arm.UpdateAccuracyCanvas();
            arm.UpdateBoundaryCanvas();
            arm.UpdateArmCanvas();
        }

        private void BaseLengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            arm.length1 = e.NewValue;
            arm.UpdateAccuracyCanvas();
            arm.UpdateBoundaryCanvas();
            arm.UpdateArmCanvas();
        }
    }
}

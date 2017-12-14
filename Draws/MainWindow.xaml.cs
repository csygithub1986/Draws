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

namespace Draws
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        int pNum;
        Point center;
        List<Point> currentPoint;
        List<Queue<Line>> lineQueue = new List<Queue<Line>>();
        double maxLen = 400;
        double lineLen = 0;
        double lineThick;
        //Brush[] brushes = { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Yellow, Brushes.Pink, Brushes.ForestGreen, Brushes.Purple, Brushes.YellowGreen };
        //int brushIndex = 0;
        int colorMode = 0;
        double colorStep;
        double valueInHsv = 0;
        double saturationInHsv = 0;
        //hsv
        double hue = 0;
        public MainWindow()
        {
            InitializeComponent();
            for (int i = 2; i <= 101; i++)
            {
                cbNum.Items.Add(i);
            }
            cbNum.SelectedIndex = 3;
            pNum = 5;
            for (int i = 0; i < pNum; i++)
            {
                lineQueue.Add(new Queue<Line>());
            }
            //长度
            for (int i = 1; i <= 10; i++)
            {
                cbLen.Items.Add(i);
            }
            cbLen.SelectedIndex = 3;
            //粗细
            for (int i = 1; i <= 10; i++)
            {
                cbThick.Items.Add(i);
            }
            cbThick.SelectedIndex = 5;
            //步进
            cbColorStep.Items.Add(0.2);
            cbColorStep.Items.Add(0.5);
            cbColorStep.Items.Add(1);
            cbColorStep.SelectedIndex = 1;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            center = new Point(paintSurface.ActualWidth / 2, paintSurface.ActualHeight / 2);
        }

        private void Canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if (e.ButtonState == MouseButtonState.Pressed)
            //{
            currentPoint = GetCurrentPoint(e.GetPosition(paintSurface)).ToList();
            //brushIndex++;
            //if (brushIndex >= brushes.Length)
            //{
            //    brushIndex = 0;
            //}
            //}
            if (colorMode == 1)
            {
                hue += 70;//360和70相约=36和7，所以为36色，7次循环后回到最初色
            }
        }

        private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point cp0 = e.GetPosition(paintSurface);

                double len0 = Math.Sqrt((cp0.Y - currentPoint[0].Y) * (cp0.Y - currentPoint[0].Y) + (cp0.X - currentPoint[0].X) * (cp0.X - currentPoint[0].X));
                if (len0 < 0.1)
                {
                    return;
                }

                //计算颜色
                Brush strokeBrush = Brushes.Black;
                if (colorMode == 0 || colorMode == 2)
                {
                    hue += colorStep;
                    if (hue > 360)
                    {
                        hue = 0;
                    }
                }

                //亮度、饱和度随机
                valueInHsv += 0.001;
                if (valueInHsv >= 1)
                {
                    valueInHsv = 0;
                }
                double colorValue = 0.5 + Math.Abs(valueInHsv - 0.5);
                saturationInHsv += 0.002;
                if (saturationInHsv >= 1)
                {
                    saturationInHsv = 0;
                }
                double colorSaturation = 0.5 + Math.Abs(saturationInHsv - 0.5);

                ////////////////////////////////////////
                colorValue = 1;
                colorSaturation = 1;
                ////////////////////////////////////////

                System.Drawing.Color hsvColor = ColorFromHSV(hue, colorSaturation, colorValue);
                strokeBrush = new SolidColorBrush(Color.FromRgb(hsvColor.R, hsvColor.G, hsvColor.B));

                Point[] cp = GetCurrentPoint(cp0);
                for (int i = 0; i < pNum; i++)
                {
                    if (colorMode == 2)
                    {
                        double hueTemp = hue + i * 360 / pNum / 2;
                        if (hueTemp > 360)
                        {
                            hueTemp -= 360;
                        }
                        hsvColor = ColorFromHSV(hueTemp, colorSaturation, colorValue);
                        strokeBrush = new SolidColorBrush(Color.FromRgb(hsvColor.R, hsvColor.G, hsvColor.B));
                    }

                    Line line = new Line();
                    line.Stroke = strokeBrush;
                    line.StrokeThickness = lineThick;
                    line.StrokeLineJoin = PenLineJoin.Round;
                    line.X1 = currentPoint[i].X;
                    line.Y1 = currentPoint[i].Y;
                    line.X2 = cp[i].X;
                    line.Y2 = cp[i].Y;
                    lineQueue[i].Enqueue(line);
                    paintSurface.Children.Add(line);
                }
                lineLen += len0;
                currentPoint = cp.ToList();

                while (lineLen > maxLen)
                {
                    Line headLine = null;
                    for (int i = 0; i < pNum; i++)
                    {
                        headLine = lineQueue[i].Dequeue();
                        paintSurface.Children.Remove(headLine);
                    }
                    lineLen -= Math.Sqrt((headLine.Y2 - headLine.Y1) * (headLine.Y2 - headLine.Y1) + (headLine.X2 - headLine.X1) * (headLine.X2 - headLine.X1));
                }

                //透明度渐变
                for (int i = 0; i < pNum; i++)
                {
                    for (int j = 0; j < lineQueue[i].Count; j++)
                    {
                        //lineQueue[i].ElementAt(j).Opacity =  (j+1) * 1.0 / lineQueue[i].Count;
                        lineQueue[i].ElementAt(j).Opacity = Math.Sqrt(1.0 * (j + 1) / lineQueue[i].Count);
                    }
                }
            }
        }

        private Point[] GetCurrentPoint(Point p0)
        {
            Point[] points = new Point[pNum];

            points[0] = p0;
            double x = p0.X - center.X;
            double y = p0.Y - center.Y;
            double radius = Math.Sqrt(y * y + x * x);
            double arg0 = Math.Atan2(y, x);
            for (int i = 1; i < pNum; i++)
            {
                points[i] = new Point(radius * Math.Cos(arg0 + i * (Math.PI * 2 / pNum)) + center.X, radius * Math.Sin(arg0 + i * (Math.PI * 2 / pNum)) + center.Y);
            }
            return points;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            center = new Point(paintSurface.ActualWidth / 2, paintSurface.ActualHeight / 2);
        }

        #region toolbar



        private void cbNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pNum = int.Parse(cbNum.SelectedItem.ToString());
            lineQueue.Clear();
            for (int i = 0; i < pNum; i++)
            {
                lineQueue.Add(new Queue<Line>());
            }
            lineLen = 0;
            paintSurface.Children.Clear();
        }


        private void cbLen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            maxLen = int.Parse(cbLen.SelectedItem.ToString()) * 100;
        }

        private void cbThick_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lineThick = int.Parse(cbThick.SelectedItem.ToString());
        }

        private void cbColorMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            colorMode = cbColorMode.SelectedIndex;
        }
        private void cbColorStep_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            colorStep = double.Parse(cbColorStep.SelectedItem.ToString());
        }
        #endregion

        #region Color
        void ColorToHSV(System.Drawing.Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        System.Drawing.Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return System.Drawing.Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return System.Drawing.Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return System.Drawing.Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return System.Drawing.Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return System.Drawing.Color.FromArgb(255, t, p, v);
            else
                return System.Drawing.Color.FromArgb(255, v, p, q);
        }

        #endregion

    }
}

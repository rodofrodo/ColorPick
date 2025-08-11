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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace ColorPick
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            pickColorBtn.BorderThickness = new Thickness(0);
        }

        private void OnMouseLeft_TitleBar(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private BitmapImage GetBitmapFromFile(string fileName)
        {
            // Visual Studio couldn't let me do in one line so it's 4-line method
            return new BitmapImage(new Uri($"pack://application:,,,/ColorPick;component/{fileName}"));
        }

        private void OnMouseEnter_MINI(object sender, MouseEventArgs e)
        {
            mini_btn.Source = GetBitmapFromFile("Resources/MINI_HOV.png");
            Cursor = Cursors.Hand;
        }

        private void OnMouseLeave_MINI(object sender, MouseEventArgs e)
        {
            mini_btn.Source = GetBitmapFromFile("Resources/MINI_BTN.png");
            Cursor = Cursors.Arrow;
        }

        private void OnMouseDown_MINI(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                WindowState = WindowState.Minimized;
        }

        private void OnMouseEnter_EXIT(object sender, MouseEventArgs e)
        {
            exit_btn.Source = GetBitmapFromFile("Resources/EXIT_HOV.png");
            Cursor = Cursors.Hand;
        }

        private void OnMouseLeave_EXIT(object sender, MouseEventArgs e)
        {
            exit_btn.Source = GetBitmapFromFile("Resources/EXIT_BTN.png");
            Cursor = Cursors.Arrow;
        }

        private void OnMouseDown_EXIT(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Hide();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Animates the border
        /// </summary>
        /// <param name="start">Fires an animation; otherwise: goes back</param>
        /// <param name="border">The border getting animation</param>
        private void AnimateBorder(bool start, Border border)
        {
            Storyboard st = new Storyboard();
            ThicknessAnimation animation = new ThicknessAnimation
            {
                From = start ? new Thickness(0) : new Thickness(3),
                To = start ? new Thickness(3) : new Thickness(0),
                Duration = new Duration(TimeSpan.FromMilliseconds(150))
            };
            Storyboard.SetTargetProperty(animation, new PropertyPath(Border.BorderThicknessProperty));
            st.Children.Add(animation);
            border.BeginStoryboard(st);
        }

        private void OnMouseEnter_PickBtn(object sender, MouseEventArgs e)
        {
            pickColorLbl.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#464646"));
            AnimateBorder(true, pickColorBtn);
        }

        private void OnMouseLeave_PickBtn(object sender, MouseEventArgs e)
        {
            pickColorLbl.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
            AnimateBorder(false, pickColorBtn);
        }

        private void OnMouseEnter_PaletteBtn(object sender, MouseEventArgs e)
        {
            paletteBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
        }

        private void OnMouseLeave_PaletteBtn(object sender, MouseEventArgs e)
        {
            paletteBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202020"));
        }

        // ---
        private bool isDragging = false;
        private Point clickPosition;
        private bool isDraggingHue = false;
        private Point clickPositionHue;

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            var ellipse = sender as Ellipse;
            clickPosition = e.GetPosition(ellipse);
            ellipse.CaptureMouse();
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) return;

            var ellipse = sender as Ellipse;
            var canvas = ellipse.Parent as Canvas;

            Point mousePos = e.GetPosition(canvas);

            double left = mousePos.X - clickPosition.X;
            double top = mousePos.Y - clickPosition.Y;

            // Clamp inside the canvas
            left = Math.Max(0, Math.Min(left, canvas.ActualWidth - ellipse.ActualWidth));
            top = Math.Max(0, Math.Min(top, canvas.ActualHeight - ellipse.ActualHeight));

            // Move ellipse (update UI first)
            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, top);

            // Compute center point of ellipse (in canvas coordinates)
            double centerX = left + ellipse.ActualWidth / 2.0;
            double centerY = top + ellipse.ActualHeight / 2.0;
            Point center = new Point(centerX, centerY);

            // Sample composed color at the ellipse center
            Color picked = SampleColorAt(center);

            // Apply color to marker preview and preview rect
            ellipse.Fill = new SolidColorBrush(picked);
            PreviewRect.Fill = new SolidColorBrush(picked);
        }

        private void Ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var ellipse = sender as Ellipse;
            ellipse.ReleaseMouseCapture();
        }

        private void HuePicker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDraggingHue = true;
            var ellipse = sender as Ellipse;
            clickPositionHue = e.GetPosition(ellipse);
            ellipse.CaptureMouse();
        }

        private void HuePicker_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingHue) return;

            var ellipse = sender as Ellipse;
            var canvas = ellipse.Parent as Canvas;

            Point mousePos = e.GetPosition(canvas);

            double left = mousePos.X - clickPositionHue.X;

            // Clamp horizontally inside canvas bounds
            left = Math.Max(0, Math.Min(left, canvas.ActualWidth - ellipse.ActualWidth));

            // Set horizontal position only, vertical stays fixed
            Canvas.SetLeft(ellipse, left);

            // Calculate relative position 0..1 across the gradient
            double relativePos = left / (canvas.ActualWidth - ellipse.ActualWidth);

            // Get interpolated color at relativePos
            Color color = InterpolateColor(officialGradient, relativePos);
            if (BaseColorBrush is SolidColorBrush scb)
                scb.Color = color;
            else
                ColorRect.Fill = new SolidColorBrush(color); // fallback

            // Set ellipse fill
            ellipse.Fill = new SolidColorBrush(color);
        }

        private void HuePicker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingHue = false;
            var ellipse = sender as Ellipse;
            ellipse.ReleaseMouseCapture();
        }

        private Color InterpolateColor(LinearGradientBrush brush, double offset)
        {
            // clamp
            offset = Math.Max(0, Math.Min(1, offset));

            // ensure stops are sorted by Offset
            var stops = brush.GradientStops.OrderBy(gs => gs.Offset).ToList();

            GradientStop before = stops.First();
            GradientStop after = stops.Last();

            foreach (var gs in stops)
            {
                if (gs.Offset <= offset) before = gs;
                if (gs.Offset >= offset) { after = gs; break; }
            }

            if (before == after) return before.Color;

            double range = after.Offset - before.Offset;
            double frac = range == 0 ? 0 : (offset - before.Offset) / range;

            byte a = (byte)(before.Color.A + (after.Color.A - before.Color.A) * frac);
            byte r = (byte)(before.Color.R + (after.Color.R - before.Color.R) * frac);
            byte g = (byte)(before.Color.G + (after.Color.G - before.Color.G) * frac);
            byte b = (byte)(before.Color.B + (after.Color.B - before.Color.B) * frac);

            return Color.FromArgb(a, r, g, b);
        }

        private Color AlphaBlend(Color top, Color bottom)
        {
            double ta = top.A / 255.0;
            double ba = bottom.A / 255.0;

            // out alpha
            double oa = ta + ba * (1 - ta);

            // avoid divide-by-zero; if oa==0 return transparent
            if (oa <= 0.00001)
                return Color.FromArgb(0, 0, 0, 0);

            double r = (top.R * ta + bottom.R * ba * (1 - ta)) / oa;
            double g = (top.G * ta + bottom.G * ba * (1 - ta)) / oa;
            double b = (top.B * ta + bottom.B * ba * (1 - ta)) / oa;

            return Color.FromArgb((byte)(oa * 255.0), (byte)r, (byte)g, (byte)b);
        }

        // Sample the composed color at a point (local coords inside ColorBox)
        private Color SampleColorAt(Point localPoint)
        {
            // normalize x and y in 0..1
            double xNorm = Math.Max(0, Math.Min(1, localPoint.X / ColorBox.ActualWidth));
            double yNorm = Math.Max(0, Math.Min(1, localPoint.Y / ColorBox.ActualHeight));

            // base color (assumed solid)
            Color baseColor = (BaseColorBrush as SolidColorBrush)?.Color ?? Colors.Black;

            // sample white overlay at x (horizontal)
            Color whiteSample = InterpolateColor(WhiteOverlayBrush, xNorm);

            // composite white over base
            Color afterWhite = AlphaBlend(whiteSample, baseColor);

            // sample black overlay at y (vertical)
            Color blackSample = InterpolateColor(BlackOverlayBrush, yNorm);

            // composite black over result
            Color final = AlphaBlend(blackSample, afterWhite);

            return final;
        }
    }
}

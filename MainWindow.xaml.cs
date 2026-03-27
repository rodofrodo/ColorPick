using System;
using System.Collections.Generic;
using System.Dynamic;
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
            clickPosition = e.GetPosition(mainEllipse);
            mainEllipse.CaptureMouse();
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) return;

            Point mousePos = e.GetPosition(mainCanvas);
            double left = mousePos.X - clickPosition.X;
            double top = mousePos.Y - clickPosition.Y;
            left = Math.Max(0, Math.Min(left, mainCanvas.ActualWidth - mainEllipse.ActualWidth));
            top = Math.Max(0, Math.Min(top, mainCanvas.ActualHeight - mainEllipse.ActualHeight));

            Canvas.SetLeft(mainEllipse, left);
            Canvas.SetTop(mainEllipse, top);

            double centerX = left + mainEllipse.ActualWidth / 2.0;
            double centerY = top + mainEllipse.ActualHeight / 2.0;
            Point center = new Point(centerX, centerY);

            Color picked = SampleColorAtCenter(center, mainEllipse.ActualWidth, mainEllipse.ActualHeight);
            mainEllipse.Fill = new SolidColorBrush(picked);
            PreviewRect.Fill = new SolidColorBrush(picked);
            SetInfo(picked);
        }

        private void Ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            mainEllipse.ReleaseMouseCapture();
        }

        private void HuePicker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDraggingHue = true;
            clickPositionHue = e.GetPosition(hueEllipse);
            hueEllipse.CaptureMouse();
        }

        private void HuePicker_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingHue) return;

            Point mousePos = e.GetPosition(hueCanvas);
            double left = mousePos.X - clickPositionHue.X;
            left = Math.Max(0, Math.Min(left, hueCanvas.ActualWidth - hueEllipse.ActualWidth));

            Canvas.SetLeft(hueEllipse, left);
            double relativePos = left / (hueCanvas.ActualWidth - hueEllipse.ActualWidth);

            Color color = InterpolateColor(officialGradient, relativePos);
            if (BaseColorBrush is SolidColorBrush scb)
                scb.Color = color;
            else
                ColorRect.Fill = new SolidColorBrush(color); // fallback
            hueEllipse.Fill = new SolidColorBrush(color);

            double centerX = Canvas.GetLeft(mainEllipse) + mainEllipse.ActualWidth / 2.0;
            double centerY = Canvas.GetTop(mainEllipse) + mainEllipse.ActualHeight / 2.0;
            Point center = new Point(centerX, centerY);

            Color picked = SampleColorAtCenter(center, mainEllipse.ActualWidth, mainEllipse.ActualHeight);
            mainEllipse.Fill = new SolidColorBrush(picked);
            PreviewRect.Fill = new SolidColorBrush(picked);
            SetInfo(picked);
        }

        private void HuePicker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingHue = false;
            hueEllipse.ReleaseMouseCapture();
        }

        private byte ToByteClamped(double value)
        {
            if (value >= 254.5) return 255;
            if (value <= 0.5) return 0;
            int iv = (int)Math.Round(value, MidpointRounding.AwayFromZero);
            return (byte)Math.Max(0, Math.Min(255, iv));
        }

        private CMYK ColorToCMYK(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;
            double k = 1 - Math.Max(r, Math.Max(g, b));
            double c = (1 - r - k) / (1 - k);
            double m = (1 - g - k) / (1 - k);
            double y = (1 - b - k) / (1 - k);
            // Handle black special case
            if (Math.Abs(k - 1) < 1e-10)
                c = m = y = 0;
            return new CMYK(c, m, y, k);
        }

        private HSV ColorToHSV(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;
            double h = 0;
            if (delta != 0)
            {
                if (max == r)
                    h = 60 * (((g - b) / delta) % 6);
                else if (max == g)
                    h = 60 * (((b - r) / delta) + 2);
                else
                    h = 60 * (((r - g) / delta) + 4);
            }
            if (h < 0) h += 360;
            double s = (max == 0) ? 0 : delta / max;
            double v = max;
            return new HSV(h, s, v);
        }

        public HSL ColorToHSL(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;
            double h = 0;
            if (delta != 0)
            {
                if (max == r)
                    h = 60 * (((g - b) / delta) % 6);
                else if (max == g)
                    h = 60 * (((b - r) / delta) + 2);
                else
                    h = 60 * (((r - g) / delta) + 4);
            }
            if (h < 0) h += 360;
            double l = (max + min) / 2;
            double s = (delta == 0) ? 0 : delta / (1 - Math.Abs(2 * l - 1));
            return new HSL(h, s, l);
        }

        private void SetInfo(Color color)
        {
            string hex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            string rgb = $"{color.R}, {color.G}, {color.B}";
            CMYK co = ColorToCMYK(color);
            string cmyk = $"{Math.Round(co.C * 100)}%, {Math.Round(co.M * 100)}%, " +
                $"{Math.Round(co.Y * 100)}%, {Math.Round(co.K * 100)}%";
            HSV hh = ColorToHSV(color);
            string hsv = $"{Math.Round(hh.H)}°, {Math.Round(hh.S * 100)}%, {Math.Round(hh.V * 100)}%";
            HSL hl = ColorToHSL(color);
            string hsl = $"{Math.Round(hl.H)}°, {Math.Round(hl.S * 100)}%, {Math.Round(hl.L * 100)}%";
            hexLbl.Text = hex;
            rgbLbl.Text = rgb;
            cmykLbl.Text = cmyk;
            hsvLbl.Text = hsv;
            hslLbl.Text = hsl;
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

            byte a = ToByteClamped(before.Color.A + (after.Color.A - before.Color.A) * frac);
            byte r = ToByteClamped(before.Color.R + (after.Color.R - before.Color.R) * frac);
            byte g = ToByteClamped(before.Color.G + (after.Color.G - before.Color.G) * frac);
            byte b = ToByteClamped(before.Color.B + (after.Color.B - before.Color.B) * frac);

            return Color.FromArgb(a, r, g, b);
        }

        private Color AlphaBlend(Color top, Color bottom)
        {
            if (top.A == 255) return top;   // top fully opaque
            if (top.A == 0) return bottom;  // top fully transparent

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

            return Color.FromArgb(
                ToByteClamped(oa * 255.0),
                ToByteClamped(r),
                ToByteClamped(g),
                ToByteClamped(b)
            );
        }

        private Color SampleColorAtCenter(Point center, double markerWidth, double markerHeight)
        {
            double boxW = ColorBox.ActualWidth;
            double boxH = ColorBox.ActualHeight;

            // half sizes
            double halfW = markerWidth / 2.0;
            double halfH = markerHeight / 2.0;

            // compute normalized coords using the *range of valid centers*
            double denomX = Math.Max(1.0, boxW - markerWidth); // avoid div by zero
            double denomY = Math.Max(1.0, boxH - markerHeight);

            double xNorm = (center.X - halfW) / denomX;
            double yNorm = (center.Y - halfH) / denomY;

            // clamp
            xNorm = Math.Max(0.0, Math.Min(1.0, xNorm));
            yNorm = Math.Max(0.0, Math.Min(1.0, yNorm));

            // base color
            Color baseColor = BaseColorBrush?.Color ?? Colors.Black;

            // sample overlays
            Color whiteSample = InterpolateColor(WhiteOverlayBrush, xNorm);
            Color afterWhite = AlphaBlend(whiteSample, baseColor);

            Color blackSample = InterpolateColor(BlackOverlayBrush, yNorm);
            Color final = AlphaBlend(blackSample, afterWhite);

            return final;
        }

        public Color HSVToColor(double h, double s, double v)
        {
            double c = v * s;
            double x = c * (1 - Math.Abs(((h / 60.0) % 2) - 1));
            double m = v - c;

            double r1 = 0, g1 = 0, b1 = 0;
            if (h < 60) { r1 = c; g1 = x; b1 = 0; }
            else if (h < 120) { r1 = x; g1 = c; b1 = 0; }
            else if (h < 180) { r1 = 0; g1 = c; b1 = x; }
            else if (h < 240) { r1 = 0; g1 = x; b1 = c; }
            else if (h < 300) { r1 = x; g1 = 0; b1 = c; }
            else { r1 = c; g1 = 0; b1 = x; }

            byte R = (byte)Math.Round((r1 + m) * 255.0);
            byte G = (byte)Math.Round((g1 + m) * 255.0);
            byte B = (byte)Math.Round((b1 + m) * 255.0);
            return Color.FromRgb(R, G, B);
        }

        private void OnKeyDown_TEXTBOX(object sender, KeyEventArgs e)
        {
            if (!(sender is TextBox tb)) return;
            if (tb.Name == "rgbLbl")
            {
                TryApplyColorFromTextBox(tb, ColorType.Rgb);
            }
        }

        private void OnLostFocus_TEXTBOX(object sender, RoutedEventArgs e)
        {
        }

        private void TryApplyColorFromTextBox(TextBox tb, ColorType ct)
        {
            if (string.IsNullOrWhiteSpace(tb.Text)) return;
            try
            {
                Color color = Color.FromRgb(0, 0, 0);
                switch (ct)
                {
                    case ColorType.Rgb:
                        string[] rgbParts = tb.Text.Split(',');
                        if (rgbParts.Length != 3) return;
                        byte r = byte.Parse(rgbParts[0].Trim());
                        byte g = byte.Parse(rgbParts[1].Trim());
                        byte b = byte.Parse(rgbParts[2].Trim());
                        color = Color.FromRgb(r, g, b);
                        break;
                }
                //SetPickersFromColor(color);
                mainEllipse.Fill = new SolidColorBrush(color);
                PreviewRect.Fill = new SolidColorBrush(color);
            }
            catch
            {
            }
        }
    }
}

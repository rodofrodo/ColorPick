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

            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, top);
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
        }

        private void HuePicker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingHue = false;
            var ellipse = sender as Ellipse;
            ellipse.ReleaseMouseCapture();
        }
    }
}

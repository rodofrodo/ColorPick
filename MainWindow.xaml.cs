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
    }
}

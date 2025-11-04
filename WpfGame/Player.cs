using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace WpfGame
{
    public class Player
    {
        public Rectangle Visual { get; private set; }
        public int Lives { get; set; }
        public double Speed { get; set; } = 6;
        public bool MoveLeft { get; set; }
        public bool MoveRight { get; set; }
        public bool Shooting { get; set; }

        private Canvas gameCanvas;
        private int initialLives = 3;

        public Player(Canvas canvas)
        {
            gameCanvas = canvas;
            Lives = initialLives;
            CreateVisual();
        }

        private void CreateVisual()
        {
            Visual = new Rectangle()
            {
                Width = 40,
                Height = 40,
                Fill = CreatePlayerAppearance(),
                RadiusX = 4,
                RadiusY = 4
            };
            gameCanvas.Children.Add(Visual);
        }

        private Brush CreatePlayerAppearance()
        {
            try
            {
                var imageBrush = new ImageBrush();
                imageBrush.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                    new System.Uri("pack://application:,,,/Images/player.png"));
                imageBrush.Stretch = Stretch.Uniform;
                return imageBrush;
            }
            catch
            {
                return new SolidColorBrush(Colors.LightGreen);
            }
        }

        public void SetPosition(double x, double y)
        {
            Canvas.SetLeft(Visual, x);
            Canvas.SetTop(Visual, y);
        }

        public void UpdatePosition()
        {
            double px = Canvas.GetLeft(Visual);
            if (MoveLeft) px -= Speed;
            if (MoveRight) px += Speed;
            px = Math.Max(0, Math.Min(gameCanvas.ActualWidth - Visual.Width, px));
            Canvas.SetLeft(Visual, px);
        }

        public void HandleKeyDown(Key key)
        {
            if (key == Key.Left || key == Key.A) MoveLeft = true;
            if (key == Key.Right || key == Key.D) MoveRight = true;
            if (key == Key.Space) Shooting = true;
        }

        public void HandleKeyUp(Key key)
        {
            if (key == Key.Left || key == Key.A) MoveLeft = false;
            if (key == Key.Right || key == Key.D) MoveRight = false;
            if (key == Key.Space) Shooting = false;
        }

        public void AnimateSpawn()
        {
            var animation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            Visual.BeginAnimation(Rectangle.OpacityProperty, animation);
        }

        // НОВЫЙ МЕТОД: Сброс состояния игрока
        public void Reset()
        {
            Lives = initialLives;
            MoveLeft = false;
            MoveRight = false;
            Shooting = false;

            // Сбрасываем позицию
            if (Visual != null && gameCanvas.ActualWidth > 0)
            {
                Visual.Opacity = 1;
                SetPosition((gameCanvas.ActualWidth - Visual.Width) / 2,
                           gameCanvas.ActualHeight - Visual.Height - 20);
            }
        }
    }
}
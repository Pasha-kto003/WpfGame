using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfGame
{
    public class Boss
    {
        public Rectangle Visual { get; private set; }
        public int Health { get; set; }
        public int MaxHealth { get; } = 20;
        public double Direction { get; set; } = 1;
        public bool IsAlive => Visual != null;
        public bool IsInfiniteBoss { get; }

        private Canvas gameCanvas;

        public Boss(Canvas canvas, bool isInfiniteBoss)
        {
            gameCanvas = canvas;
            IsInfiniteBoss = isInfiniteBoss;

            if (isInfiniteBoss)
            {
                MaxHealth = 1; // НОВОЕ: в бесконечном режиме босс уничтожается с одного попадания
                Health = 1;
                CreateInfiniteBossVisual();
            }
            else
            {
                MaxHealth = 20;
                Health = 20;
                CreateVisual();
            }
        }

        private void CreateVisual()
        {
            Visual = new Rectangle()
            {
                Width = 150,
                Height = 80, 
                Fill = CreateBossAppearance(),
                Stroke = Brushes.Transparent,
                StrokeThickness = 3,
            };
            gameCanvas.Children.Add(Visual);
            Canvas.SetLeft(Visual, (gameCanvas.ActualWidth - Visual.Width) / 2);
            Canvas.SetTop(Visual, 60);
        }

        private void CreateInfiniteBossVisual()
        {
            Visual = new Rectangle()
            {
                Width = 120,
                Height = 50,
                Fill = CreateInfiniteBossAppearance(),
                Stroke = Brushes.Gold,
                StrokeThickness = 2,
            };
            gameCanvas.Children.Add(Visual);
            Canvas.SetLeft(Visual, (gameCanvas.ActualWidth - Visual.Width) / 2);
            Canvas.SetTop(Visual, 40);
        }

        private Brush CreateBossAppearance()
        {
            try
            {
                var imageBrush = new ImageBrush();
                imageBrush.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                    new System.Uri("pack://application:,,,/Images/Boss.png"));
                imageBrush.Stretch = Stretch.Uniform;
                return imageBrush;
            }
            catch
            {
                return new SolidColorBrush(Colors.MediumPurple);
            }
        }

        private Brush CreateInfiniteBossAppearance()
        {
            try
            {
                var imageBrush = new ImageBrush();
                imageBrush.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                    new System.Uri("pack://application:,,,/Images/Boss.png"));
                imageBrush.Stretch = Stretch.Uniform;
                return imageBrush;
            }
            catch
            {
                // Fallback цвет для босса бесконечного режима
                return new SolidColorBrush(Colors.Gold);
            }
        }

        public void Update()
        {
            if (!IsAlive) return;

            double bx = Canvas.GetLeft(Visual);
            bx += Direction * 2;

            if (bx <= 10 || bx + Visual.Width >= gameCanvas.ActualWidth - 10)
            {
                Direction *= -1;
            }

            Canvas.SetLeft(Visual, bx);
        }

        public void Remove()
        {
            if (Visual != null)
            {
                gameCanvas.Children.Remove(Visual);
                Visual = null;
            }
        }

        public Rect GetBounds()
        {
            return Visual != null
                ? new Rect(Canvas.GetLeft(Visual), Canvas.GetTop(Visual), Visual.Width, Visual.Height)
                : new Rect();
        }

        public (double x, double y)? GetPosition()
        {
            if (!IsAlive) return null;

            return (Canvas.GetLeft(Visual), Canvas.GetTop(Visual));
        }
    }
}
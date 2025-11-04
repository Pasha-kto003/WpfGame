using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace WpfGame
{
    public class AnimationManager
    {
        private Canvas gameCanvas;
        private Random rnd = new Random();
        private SoundManager soundManager; // ДОБАВЛЕНО

        public AnimationManager(Canvas canvas, SoundManager soundManager) // ИЗМЕНЕНО
        {
            gameCanvas = canvas;
            this.soundManager = soundManager; // ДОБАВЛЕНО
        }

        public void CreateExplosion(double x, double y, double size = 30)
        {
            soundManager.PlaySound("explosion"); // ДОБАВЛЕНО

            var explosion = new Ellipse()
            {
                Width = size * 0.5,
                Height = size * 0.5,
                Fill = new RadialGradientBrush(Colors.Yellow, Colors.Red),
                Opacity = 1
            };
            gameCanvas.Children.Add(explosion);
            Canvas.SetLeft(explosion, x - explosion.Width / 2);
            Canvas.SetTop(explosion, y - explosion.Height / 2);

            var scaleAnimation = new DoubleAnimation(size * 0.5, size, TimeSpan.FromSeconds(0.3));
            var opacityAnimation = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.4));

            scaleAnimation.Completed += (s, e) =>
            {
                if (gameCanvas.Children.Contains(explosion))
                    gameCanvas.Children.Remove(explosion);
            };

            explosion.BeginAnimation(Ellipse.WidthProperty, scaleAnimation);
            explosion.BeginAnimation(Ellipse.HeightProperty, scaleAnimation);
            explosion.BeginAnimation(Ellipse.OpacityProperty, opacityAnimation);
        }

        public void CreateBossHitEffect(Rectangle boss)
        {
            soundManager.PlaySound("boss_hit"); // ДОБАВЛЕНО

            var hitEffect = new Rectangle()
            {
                Width = boss.Width,
                Height = boss.Height,
                Fill = Brushes.White,
                Opacity = 0.7
            };
            gameCanvas.Children.Add(hitEffect);
            Canvas.SetLeft(hitEffect, Canvas.GetLeft(boss));
            Canvas.SetTop(hitEffect, Canvas.GetTop(boss));

            var animation = new DoubleAnimation(0.7, 0, TimeSpan.FromSeconds(0.2));
            animation.Completed += (s, e) => gameCanvas.Children.Remove(hitEffect);
            hitEffect.BeginAnimation(Rectangle.OpacityProperty, animation);
        }
    }
}
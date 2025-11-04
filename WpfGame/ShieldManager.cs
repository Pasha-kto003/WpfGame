using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WpfGame
{
    public class ShieldManager
    {
        public List<Shield> Shields { get; private set; } = new List<Shield>();
        private Canvas gameCanvas;
        private Random rnd = new Random();
        private AnimationManager animate;
        private GameStateManager gameState;

        public ShieldManager(Canvas canvas, GameStateManager gameStateManager) // ИЗМЕНЕНО: добавляем gameState
        {
            gameCanvas = canvas;
            gameState = gameStateManager;
        }

        public void CreateShields()
        {
            ClearShields();

            if (gameCanvas.ActualWidth == 0 || gameCanvas.ActualHeight == 0)
            {
                gameCanvas.Loaded += (s, e) => CreateShields();
                return;
            }

            double shieldWidth = 60;
            double shieldHeight = 20;
            double shieldY = gameCanvas.ActualHeight - 120;

            int shieldCount = gameState.IsInfiniteMode ? 5 : 2;
            int shieldHealth = gameState.IsInfiniteMode ? 30 : 5;

            for (int i = 0; i < 2; i++)
            {
                var shield = new Shield()
                {
                    Health = 5,
                    MaxHealth = 5,
                    Visual = new Rectangle()
                    {
                        Width = shieldWidth,
                        Height = shieldHeight,
                        Fill = CreateShieldGradient(),
                        Stroke = Brushes.Cyan,
                        StrokeThickness = 2,
                        RadiusX = 8,
                        RadiusY = 8
                    }
                };

                gameCanvas.Children.Add(shield.Visual);

                double shieldSpacing = gameCanvas.ActualWidth / (shieldCount + 1);
                double shieldX = shieldSpacing * (i + 1) - shieldWidth / 2;

                Canvas.SetLeft(shield.Visual, shieldX);
                Canvas.SetTop(shield.Visual, shieldY);

                Shields.Add(shield);
            }
        }

        private LinearGradientBrush CreateShieldGradient()
        {
            var gradient = new LinearGradientBrush();
            gradient.StartPoint = new Point(0, 0);
            gradient.EndPoint = new Point(0, 1);
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(100, 200, 255), 0));
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(0, 100, 200), 0.5));
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(0, 50, 150), 1));
            return gradient;
        }

        public void CheckCollisions(BulletManager bulletManager)
        {
            for (int i = bulletManager.EnemyBullets.Count - 1; i >= 0; i--)
            {
                var bullet = bulletManager.EnemyBullets[i];
                Rect bulletRect = new Rect(Canvas.GetLeft(bullet), Canvas.GetTop(bullet), bullet.Width, bullet.Height);

                foreach (var shield in Shields.ToList())
                {
                    if (shield.Health <= 0) continue;

                    Rect shieldRect = new Rect(Canvas.GetLeft(shield.Visual), Canvas.GetTop(shield.Visual),
                                             shield.Visual.Width, shield.Visual.Height);

                    if (bulletRect.IntersectsWith(shieldRect))
                    {
                        shield.Health--;
                        UpdateShieldAppearance(shield);
                        bulletManager.RemoveBullet(bullet, bulletManager.EnemyBullets);
                        break;
                    }
                }
            }

            for (int i = bulletManager.PlayerBullets.Count - 1; i >= 0; i--)
            {
                var bullet = bulletManager.PlayerBullets[i];
                Rect bulletRect = new Rect(Canvas.GetLeft(bullet), Canvas.GetTop(bullet), bullet.Width, bullet.Height);

                foreach (var shield in Shields.ToList())
                {
                    if (shield.Health <= 0) continue;

                    Rect shieldRect = new Rect(Canvas.GetLeft(shield.Visual), Canvas.GetTop(shield.Visual),
                                             shield.Visual.Width, shield.Visual.Height);

                    if (bulletRect.IntersectsWith(shieldRect))
                    {
                        bulletManager.RemoveBullet(bullet, bulletManager.PlayerBullets);
                        break;
                    }
                }
            }
        }

        private void UpdateShieldAppearance(Shield shield)
        {
            if (shield.Health <= 0)
            {
                CreateShieldDestructionEffect(shield.Visual);
                gameCanvas.Children.Remove(shield.Visual);
                Shields.Remove(shield);
                return;
            }

            double healthRatio = (double)shield.Health / shield.MaxHealth;
            Color shieldColor = healthRatio > 0.6 ? Color.FromRgb(100, 200, 255) :
                              healthRatio > 0.3 ? Color.FromRgb(255, 200, 100) :
                              Color.FromRgb(255, 100, 100);

            var gradient = CreateShieldGradientWithColor(shieldColor);
            shield.Visual.Fill = gradient;

            var hitAnimation = new DoubleAnimation(1, 0.5, TimeSpan.FromSeconds(0.1));
            hitAnimation.AutoReverse = true;
            shield.Visual.BeginAnimation(Rectangle.OpacityProperty, hitAnimation);
        }

        private void CreateShieldDestructionEffect(Rectangle shield)
        {
            double x = Canvas.GetLeft(shield) + shield.Width / 2;
            double y = Canvas.GetTop(shield) + shield.Height / 2;

            for (int i = 0; i < 3; i++)
            {
                var explosionTimer = new System.Windows.Threading.DispatcherTimer()
                {
                    Interval = System.TimeSpan.FromSeconds(i * 0.1)
                };
                explosionTimer.Tick += (s, e) =>
                {
                    explosionTimer.Stop();
                };
                explosionTimer.Start();
            }
        }

        private LinearGradientBrush CreateShieldGradientWithColor(Color baseColor)
        {
            var gradient = new LinearGradientBrush();
            gradient.StartPoint = new Point(0, 0);
            gradient.EndPoint = new Point(0, 1);
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(
                (byte)(baseColor.R * 1.2), (byte)(baseColor.G * 1.2), (byte)(baseColor.B * 1.2)), 0));
            gradient.GradientStops.Add(new GradientStop(baseColor, 0.5));
            gradient.GradientStops.Add(new GradientStop(Color.FromRgb(
                (byte)(baseColor.R * 0.8), (byte)(baseColor.G * 0.8), (byte)(baseColor.B * 0.8)), 1));
            return gradient;
        }

        public void ClearShields()
        {
            foreach (var shield in Shields)
            {
                if (gameCanvas.Children.Contains(shield.Visual))
                {
                    gameCanvas.Children.Remove(shield.Visual);
                }
            }
            Shields.Clear();
        }
    }

    public class Shield
    {
        public Rectangle Visual { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
    }
}
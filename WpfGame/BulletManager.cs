using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace WpfGame
{
    public class BulletManager
    {
        public List<Rectangle> PlayerBullets { get; private set; } = new List<Rectangle>();
        public List<Rectangle> EnemyBullets { get; private set; } = new List<Rectangle>();

        private Canvas gameCanvas;
        private Random rnd = new Random();
        private SoundManager soundManager; // ДОБАВЛЕНО

        public BulletManager(Canvas canvas, SoundManager soundManager) // ИЗМЕНЕНО
        {
            gameCanvas = canvas;
            this.soundManager = soundManager; // ДОБАВЛЕНО
        }

        public void ShootPlayerBullet(double playerX, double playerY, double playerWidth)
        {
            if (PlayerBullets.Count > 3) return;

            var bullet = new Rectangle()
            {
                Width = 10,
                Height = 20,
                Fill = CreatePlayerBulletAppearance()
            };
            gameCanvas.Children.Add(bullet);

            double bx = playerX + playerWidth / 2 - bullet.Width / 2;
            double by = playerY - bullet.Height - 2;

            Canvas.SetLeft(bullet, bx);
            Canvas.SetTop(bullet, by);
            PlayerBullets.Add(bullet);
            soundManager.PlaySound("shoot");
        }

        public void ShootEnemyBullet(double shooterX, double shooterY, double shooterWidth, double shooterHeight)
        {
            var bullet = new Rectangle()
            {
                Width = 10,
                Height = 20,
                Fill = CreateEnemyBulletAppearance()
            };
            gameCanvas.Children.Add(bullet);

            double bx = shooterX + shooterWidth / 2 - bullet.Width / 2;
            double by = shooterY + shooterHeight + 4;

            Canvas.SetLeft(bullet, bx);
            Canvas.SetTop(bullet, by);
            EnemyBullets.Add(bullet);
            soundManager.PlaySound("shoot");
        }

        public void UpdateBullets(double playerBulletSpeed, double enemyBulletSpeed)
        {

            for (int i = PlayerBullets.Count - 1; i >= 0; i--)
            {
                var bullet = PlayerBullets[i];
                Canvas.SetTop(bullet, Canvas.GetTop(bullet) - playerBulletSpeed);
                if (Canvas.GetTop(bullet) < -10)
                {
                    RemoveBullet(bullet, PlayerBullets);
                }
            }

            for (int i = EnemyBullets.Count - 1; i >= 0; i--)
            {
                var bullet = EnemyBullets[i];
                Canvas.SetTop(bullet, Canvas.GetTop(bullet) + enemyBulletSpeed);
                if (Canvas.GetTop(bullet) > gameCanvas.ActualHeight + 10)
                {
                    RemoveBullet(bullet, EnemyBullets);
                }
            }
        }

        public void RemoveBullet(Rectangle bullet, List<Rectangle> bulletList)
        {
            gameCanvas.Children.Remove(bullet);
            bulletList.Remove(bullet);
        }

        private Brush CreatePlayerBulletAppearance()
        {
            try
            {
                var imageBrush = new ImageBrush();
                imageBrush.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                    new System.Uri("pack://application:,,,/Images/bulletPl.png"));
                imageBrush.Stretch = Stretch.Uniform;
                return imageBrush;
            }
            catch
            {
                return new SolidColorBrush(Colors.LightBlue);
            }
        }

        private Brush CreateEnemyBulletAppearance()
        {
            try
            {
                var imageBrush = new ImageBrush();
                imageBrush.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                    new System.Uri("pack://application:,,,/Images/bulletPl.png"));
                imageBrush.Stretch = Stretch.Uniform;
                return imageBrush;
            }
            catch
            {
                return new SolidColorBrush(Colors.Yellow);
            }
        }

        public void ClearAllBullets()
        {
            foreach (var bullet in PlayerBullets.ToArray()) RemoveBullet(bullet, PlayerBullets);
            foreach (var bullet in EnemyBullets.ToArray()) RemoveBullet(bullet, EnemyBullets);
        }
    }
}
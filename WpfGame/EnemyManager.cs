using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;

namespace WpfGame
{
    public class EnemyManager
    {
        public List<Enemy> Enemies { get; private set; } = new List<Enemy>();
        public double Direction { get; set; } = 1;

        public bool BossFight { get; private set; }
        public Boss Boss { get; private set; }
        public bool IsBossAlive => Boss != null && Boss.Visual != null;

        public int CurrentWave { get; private set; } = 1;
        public double EnemySpeedMultiplier { get; private set; } = 1.0;

        private Canvas gameCanvas;
        private Random rnd = new Random();
        private GameStateManager gameState;

        public EnemyManager(Canvas canvas, GameStateManager gameStateManager)
        {
            gameCanvas = canvas;
            gameState = gameStateManager;
        }

        public void SpawnEnemiesForLevel(int level)
        {
            ClearEnemies();
            BossFight = false;
            CurrentWave = 1; // Сбрасываем волну
            EnemySpeedMultiplier = 1.0; // Сбрасываем множитель скорости

            if (level <= 3)
            {
                SpawnRegularEnemies(level);
            }
            else if (level == 4)
            {
                SpawnBoss();
            }
            else if (level == 5) // НОВОЕ: бесконечный режим
            {
                SpawnInfiniteWave();
            }
        }

        public void SpawnInfiniteWave()
        {
            ClearEnemies();
            BossFight = false;

            int rows = 2 + (CurrentWave / 2); // Увеличиваем ряды с каждой волной
            int cols = 4 + CurrentWave; // Увеличиваем колонки с каждой волной
            double marginX = 20;

            // НОВОЕ: враги появляются ближе к игроку с каждой волной
            double startY = 20 + (CurrentWave * 5);
            startY = Math.Min(startY, gameCanvas.ActualHeight * 0.4); // Ограничиваем максимальную высоту

            double spacingX = (gameCanvas.ActualWidth - marginX * 2) / cols;
            double spacingY = 35;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var enemy = new Enemy(gameCanvas,
                        marginX + c * spacingX,
                        startY + r * spacingY,
                        3); // Все враги 3 уровня в бесконечном режиме

                    Enemies.Add(enemy);
                }
            }

            // Увеличиваем скорость врагов с каждой волной
            EnemySpeedMultiplier = 1.0 + (CurrentWave * 0.1);
            CurrentWave++;
        }

        public void SpawnInfiniteBoss()
        {
            BossFight = true;
            Boss = new Boss(gameCanvas, true); // true - флаг бесконечного режима
        }

        private void SpawnRegularEnemies(int level)
        {
            int rows = 3 + level;
            int cols = 6 + level;
            double marginX = 20;
            double marginY = 20;
            double spacingX = (gameCanvas.ActualWidth - marginX * 2) / cols;
            double spacingY = 40;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var color = Color.FromRgb(
                        (byte)(180 - level * 30),
                        (byte)(100 + level * 30),
                        (byte)(50 + level * 20)
                    );

                    var enemy = new Enemy(gameCanvas,
                        marginX + c * spacingX,
                        marginY + r * spacingY,
                        level);

                    Enemies.Add(enemy);
                }
            }
        }

        private void SpawnBoss()
        {
            BossFight = true;
            Boss = new Boss(gameCanvas, false);
        }

        public void RemoveBoss()
        {
            if (Boss != null)
            {
                Boss.Remove();
                Boss = null;
            }
            BossFight = false;
        }

        public void UpdateEnemies(double baseSpeed)
        {
            if (BossFight && IsBossAlive)
            {
                Boss.Update();
                return;
            }

            if (Enemies.Count == 0)
            {
                // В бесконечном режиме спавним новую волну
                if (gameState.IsInfiniteMode)
                {
                    SpawnInfiniteWave();
                }
                return;
            }

            double leftMost = Enemies.Min(en => Canvas.GetLeft(en.Visual));
            double rightMost = Enemies.Max(en => Canvas.GetLeft(en.Visual) + en.Visual.Width);

            if (rightMost >= gameCanvas.ActualWidth - 10 && Direction > 0) Direction = -1;
            if (leftMost <= 10 && Direction < 0) Direction = 1;

            // НОВОЕ: применяем множитель скорости в бесконечном режиме
            double actualSpeed = gameState.IsInfiniteMode ? baseSpeed * EnemySpeedMultiplier : baseSpeed;

            foreach (var enemy in Enemies)
            {
                enemy.Move(Direction * actualSpeed);
            }
        }

        public List<Enemy> GetShootingEnemies()
        {
            if (Enemies.Count == 0) return new List<Enemy>();

            // Находим минимальную Y-координату (верхний ряд)
            double minY = Enemies.Min(en => Canvas.GetTop(en.Visual));

            // Возвращаем только врагов из верхнего ряда
            return Enemies.Where(en => Math.Abs(Canvas.GetTop(en.Visual) - minY) < 5).ToList();
        }

        public Enemy GetRandomShooter()
        {
            var shooters = GetShootingEnemies();
            if (shooters.Count == 0) return null;
            return shooters[rnd.Next(shooters.Count)];
        }

        public void RemoveEnemy(Enemy enemy)
        {
            enemy.Remove(gameCanvas);
            Enemies.Remove(enemy);
        }

        public void ClearEnemies()
        {
            foreach (var enemy in Enemies)
            {
                enemy.Remove(gameCanvas);
            }
            Enemies.Clear();

            if (Boss != null)
            {
                Boss.Remove();
                Boss = null;
            }
        }

        public bool IsAnyEnemyNearPlayer(double playerY)
        {
            return Enemies.Any(enemy => Canvas.GetTop(enemy.Visual) + enemy.Visual.Height >= playerY - 10);
        }
    }
}
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfGame
{
    public class GameManager
    {
        private Player player;
        private EnemyManager enemyManager;
        private BulletManager bulletManager;
        private ShieldManager shieldManager;
        private AnimationManager animationManager;
        private GameStateManager gameState;
        private UIManager uiManager;
        private CollisionManager collisionManager;
        private Random rnd = new Random();
        private DateTime lastBossSpawnTime = DateTime.MinValue; // НОВОЕ: время последнего спавна босса

        public GameManager(Player player, EnemyManager enemyManager, BulletManager bulletManager,
                         ShieldManager shieldManager, AnimationManager animationManager,
                         GameStateManager gameState, UIManager uiManager, CollisionManager collisionManager)
        {
            this.player = player;
            this.enemyManager = enemyManager;
            this.bulletManager = bulletManager;
            this.shieldManager = shieldManager;
            this.animationManager = animationManager;
            this.gameState = gameState;
            this.uiManager = uiManager;
            this.collisionManager = collisionManager;
        }

        public void UpdateGame()
        {
            player.UpdatePosition();

            if (player.Shooting)
            {
                var playerPos = Canvas.GetLeft(player.Visual);
                var playerTop = Canvas.GetTop(player.Visual);
                bulletManager.ShootPlayerBullet(playerPos, playerTop, player.Visual.Width);
                player.Shooting = false;
            }

            bulletManager.UpdateBullets(10, 6);

            double enemySpeed = 1.0 + (gameState.Level * 0.3);
            enemyManager.UpdateEnemies(enemySpeed);

            HandleEnemyShooting();

            HandleInfiniteBoss();

            collisionManager.CheckAllCollisions();

            CheckGameConditions();
        }

        private void HandleEnemyShooting()
        {
            if (enemyManager.BossFight)
            {
                // Стрельба босса
                if (enemyManager.Boss != null && rnd.NextDouble() < 0.05)
                {
                    var boss = enemyManager.Boss;
                    var bossPos = Canvas.GetLeft(boss.Visual);
                    var bossTop = Canvas.GetTop(boss.Visual);
                    bulletManager.ShootEnemyBullet(bossPos, bossTop,
                        boss.Visual.Width, boss.Visual.Height);
                }
            }
            else
            {
                // НОВОЕ: в бесконечном режиме враги стреляют чаще
                double shootChance = gameState.IsInfiniteMode ?
                    0.03 + (0.01 * enemyManager.CurrentWave) :
                    0.02 + (0.02 * gameState.Level);

                if (rnd.NextDouble() < shootChance)
                {
                    var shooter = enemyManager.GetRandomShooter();
                    if (shooter != null)
                    {
                        var shooterPos = Canvas.GetLeft(shooter.Visual);
                        var shooterTop = Canvas.GetTop(shooter.Visual);
                        bulletManager.ShootEnemyBullet(shooterPos, shooterTop,
                            shooter.Visual.Width, shooter.Visual.Height);
                    }
                }
            }
        }

        private void HandleInfiniteBoss()
        {
            if (gameState.IsInfiniteMode && gameState.ShouldSpawnInfiniteBoss())
            {
                // Спавним босса, если его нет и прошло достаточно времени
                if (!enemyManager.BossFight &&
                    (DateTime.Now - lastBossSpawnTime).TotalMinutes >= 3)
                {
                    enemyManager.SpawnInfiniteBoss();
                    lastBossSpawnTime = DateTime.Now;
                }
            }
            else if (enemyManager.BossFight && enemyManager.Boss != null && enemyManager.Boss.IsInfiniteBoss)
            {
                // Убираем босса, когда время вышло
                enemyManager.RemoveBoss();
            }
        }

        private void CheckGameConditions()
        {
            if (collisionManager.ShouldAdvanceLevel())
            {
                gameState.NextLevel();
                if (gameState.Level > 4)
                {
                    gameState.TriggerWinGame();
                }
                else
                {
                    SpawnEnemiesForLevel(gameState.Level);
                }
            }
        }

        public void SpawnEnemiesForLevel(int level)
        {
            enemyManager.SpawnEnemiesForLevel(level);
            uiManager.UpdateLevel(level);

            if (enemyManager.BossFight)
            {
                uiManager.ShowBossHealthBar();
                uiManager.UpdateBossHealthBarPosition((player.Visual.Parent as Canvas).ActualWidth);
                uiManager.UpdateBossHealthBar(1.0);
            }
            else
            {
                uiManager.HideBossHealthBar();
            }
        }

        public void StartInfiniteMode()
        {
            gameState.StartInfiniteMode();
            player.Reset(); // Явный вызов Reset для бесконечного режима
            uiManager.UpdateScore(gameState.Score);
            uiManager.UpdateLives(player.Lives);
            uiManager.UpdateLevel(5);

            bulletManager.ClearAllBullets();
            enemyManager.ClearEnemies();
            shieldManager.ClearShields();
            shieldManager.CreateShields();

            SpawnEnemiesForLevel(5);
            player.AnimateSpawn();
            gameState.StartGame();
        }

        public void ResetGame()
        {
            gameState.ResetGame();
            player.Lives = 3;
            uiManager.UpdateScore(gameState.Score);
            uiManager.UpdateLives(player.Lives);
            uiManager.UpdateLevel(gameState.Level);

            bulletManager.ClearAllBullets();
            enemyManager.ClearEnemies();
            shieldManager.ClearShields();
            shieldManager.CreateShields();

            SpawnEnemiesForLevel(gameState.Level);
            player.AnimateSpawn();
            gameState.StartGame();
        }
    }
}
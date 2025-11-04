using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace WpfGame
{
    public class CollisionManager
    {
        private Player player;
        private EnemyManager enemyManager;
        private BulletManager bulletManager;
        private ShieldManager shieldManager;
        private AnimationManager animationManager;
        private GameStateManager gameState;
        private UIManager uiManager;

        public CollisionManager(Player player, EnemyManager enemyManager, BulletManager bulletManager,
                              ShieldManager shieldManager, AnimationManager animationManager,
                              GameStateManager gameState, UIManager uiManager)
        {
            this.player = player;
            this.enemyManager = enemyManager;
            this.bulletManager = bulletManager;
            this.shieldManager = shieldManager;
            this.animationManager = animationManager;
            this.gameState = gameState;
            this.uiManager = uiManager;
        }

        public void CheckAllCollisions()
        {
            CheckShieldCollisions();
            CheckPlayerBulletCollisions();
            CheckEnemyBulletCollisions();
            CheckEnemyPlayerCollisions();
        }

        private void CheckShieldCollisions()
        {
            shieldManager.CheckCollisions(bulletManager);
        }

        private void CheckPlayerBulletCollisions()
        {
            for (int i = bulletManager.PlayerBullets.Count - 1; i >= 0; i--)
            {
                var bullet = bulletManager.PlayerBullets[i];
                Rect bulletRect = new Rect(Canvas.GetLeft(bullet), Canvas.GetTop(bullet), bullet.Width, bullet.Height);

                if (enemyManager.BossFight && enemyManager.Boss != null)
                {
                    CheckBossCollision(bullet, bulletRect);
                }
                else
                {
                    CheckEnemyCollision(bullet, bulletRect);
                }
            }
        }

        private void CheckBossCollision(Rectangle bullet, Rect bulletRect)
        {
            if (!enemyManager.IsBossAlive) return;

            Rect bossRect = enemyManager.Boss.GetBounds();
            if (bulletRect.IntersectsWith(bossRect))
            {
                animationManager.CreateBossHitEffect(enemyManager.Boss.Visual);
                bulletManager.RemoveBullet(bullet, bulletManager.PlayerBullets);
                enemyManager.Boss.Health--;

                if (enemyManager.Boss.IsInfiniteBoss)
                {
                    gameState.IncreaseScore(200);
                    uiManager.UpdateScore(gameState.Score);
                }
                else
                {
                    uiManager.UpdateBossHealthBar((double)enemyManager.Boss.Health / enemyManager.Boss.MaxHealth);
                }

                if (enemyManager.Boss.Health <= 0)
                {
                    animationManager.CreateExplosion(
                        Canvas.GetLeft(enemyManager.Boss.Visual) + enemyManager.Boss.Visual.Width / 2,
                        Canvas.GetTop(enemyManager.Boss.Visual) + enemyManager.Boss.Visual.Height / 2,
                        enemyManager.Boss.IsInfiniteBoss ? 60 : 80);

                    if (!enemyManager.Boss.IsInfiniteBoss)
                    {
                        gameState.IncreaseScore(500);
                        uiManager.UpdateScore(gameState.Score);
                        gameState.NextLevel();

                        if (gameState.Level > 4)
                        {
                            gameState.TriggerWinGame();
                        }
                    }

                    // ИСПРАВЛЕНИЕ: используем метод RemoveBoss вместо прямого доступа
                    enemyManager.RemoveBoss();
                }
            }
        }

        private void CheckEnemyCollision(Rectangle bullet, Rect bulletRect)
        {
            for (int j = enemyManager.Enemies.Count - 1; j >= 0; j--)
            {
                var enemy = enemyManager.Enemies[j];
                Rect enemyRect = enemy.GetBounds();

                if (bulletRect.IntersectsWith(enemyRect))
                {
                    animationManager.CreateExplosion(
                        Canvas.GetLeft(enemy.Visual) + enemy.Visual.Width / 2,
                        Canvas.GetTop(enemy.Visual) + enemy.Visual.Height / 2);

                    enemyManager.RemoveEnemy(enemy);
                    bulletManager.RemoveBullet(bullet, bulletManager.PlayerBullets);
                    gameState.IncreaseScore(10 * gameState.Level);
                    uiManager.UpdateScore(gameState.Score);
                    break;
                }
            }
        }

        private void CheckEnemyBulletCollisions()
        {
            for (int i = bulletManager.EnemyBullets.Count - 1; i >= 0; i--)
            {
                var bullet = bulletManager.EnemyBullets[i];
                Rect bulletRect = new Rect(Canvas.GetLeft(bullet), Canvas.GetTop(bullet), bullet.Width, bullet.Height);
                Rect playerRect = new Rect(Canvas.GetLeft(player.Visual), Canvas.GetTop(player.Visual),
                                         player.Visual.Width, player.Visual.Height);

                if (bulletRect.IntersectsWith(playerRect))
                {
                    animationManager.CreateExplosion(
                        Canvas.GetLeft(bullet) + bullet.Width / 2,
                        Canvas.GetTop(bullet) + bullet.Height / 2, 25);

                    bulletManager.RemoveBullet(bullet, bulletManager.EnemyBullets);
                    HitPlayer();
                    break;
                }
            }
        }

        private void CheckEnemyPlayerCollisions()
        {
            if (!enemyManager.BossFight && enemyManager.IsAnyEnemyNearPlayer(Canvas.GetTop(player.Visual)))
            {
                gameState.TriggerGameOver();
            }
        }

        private void HitPlayer()
        {
            player.Lives--;
            uiManager.UpdateLives(player.Lives);

            if (player.Lives <= 0)
            {
                gameState.TriggerGameOver(); 
            }
            else
            {
                player.Visual.Opacity = 0;
                player.SetPosition((player.Visual.Parent as Canvas).ActualWidth - player.Visual.Width / 2, (player.Visual.Parent as Canvas).ActualHeight - player.Visual.Height - 20);
                player.AnimateSpawn();
            }
        }

        public bool ShouldAdvanceLevel()
        {
            return !enemyManager.BossFight && enemyManager.Enemies.Count == 0;
        }
    }
}
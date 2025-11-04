using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace WpfGame
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer gameTimer;

        // Менеджеры
        private Player player;
        private EnemyManager enemyManager;
        private BulletManager bulletManager;
        private ShieldManager shieldManager;
        private AnimationManager animationManager;
        private UIManager uiManager;
        private GameStateManager gameState;
        private CollisionManager collisionManager;
        private GameManager gameManager;
        private PauseMenuManager pauseMenuManager;
        private SoundManager soundManager;

        private bool isInMainMenu = true;

        public MainWindow()
        {
            InitializeComponent();

            InitializeManagers();
            InitializeGame();

            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            CreateMainMenu();
        }

        private void InitializeManagers()
        {
            gameTimer = new DispatcherTimer();
            soundManager = new SoundManager();
            player = new Player(GameCanvas);
            gameState = new GameStateManager(gameTimer);

            enemyManager = new EnemyManager(GameCanvas, gameState);
            bulletManager = new BulletManager(GameCanvas, soundManager);
            shieldManager = new ShieldManager(GameCanvas, gameState);
            animationManager = new AnimationManager(GameCanvas, soundManager);
            uiManager = new UIManager(GameCanvas);

            gameState.GameOver += OnGameOver;
            gameState.GameWon += OnGameWon;
            gameState.ResetLives += OnResetLives;

            collisionManager = new CollisionManager(player, enemyManager, bulletManager, shieldManager,
                                                  animationManager, gameState, uiManager);
            gameManager = new GameManager(player, enemyManager, bulletManager, shieldManager,
                                        animationManager, gameState, uiManager, collisionManager);


            pauseMenuManager = new PauseMenuManager(GameCanvas, gameState, soundManager);
        }

        private void CreateMainMenu()
        {
            var mainMenu = new Border()
            {
                Width = 350,
                Height = 300,
                Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 40)),
                BorderBrush = Brushes.Cyan,
                BorderThickness = new Thickness(3),
                CornerRadius = new CornerRadius(15),
                Name = "MainMenu" // Добавляем имя для возможного удаления
            };
            GameCanvas.Children.Add(mainMenu);

            var title = new TextBlock()
            {
                Text = "SPACE INVADERS",
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Cyan,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 30)
            };

            var campaignButton = new Button()
            {
                Content = "Кампания",
                Width = 150,
                Height = 40,
                FontSize = 16,
                Background = new SolidColorBrush(Color.FromRgb(0, 100, 200)),
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 10)
            };
            campaignButton.Click += (s, e) =>
            {
                StartCampaign();
                RemoveMainMenu();
            };

            var infiniteButton = new Button()
            {
                Content = "Бесконечный режим",
                Width = 150,
                Height = 40,
                FontSize = 16,
                Background = new SolidColorBrush(Color.FromRgb(200, 100, 0)),
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 10)
            };
            infiniteButton.Click += (s, e) =>
            {
                StartInfiniteMode();
                RemoveMainMenu();
            };

            var exitButton = new Button()
            {
                Content = "Выход",
                Width = 150,
                Height = 40,
                FontSize = 16,
                Background = new SolidColorBrush(Color.FromRgb(200, 60, 60)),
                Foreground = Brushes.White
            };
            exitButton.Click += (s, e) => Application.Current.Shutdown();

            var stackPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            stackPanel.Children.Add(title);
            stackPanel.Children.Add(campaignButton);
            stackPanel.Children.Add(infiniteButton);
            stackPanel.Children.Add(exitButton);

            mainMenu.Child = stackPanel;

            // Центрируем меню после загрузки
            Loaded += (s, e) =>
            {
                Canvas.SetLeft(mainMenu, (GameCanvas.ActualWidth - mainMenu.Width) / 2);
                Canvas.SetTop(mainMenu, (GameCanvas.ActualHeight - mainMenu.Height) / 2);
            };
        }

        private void RemoveMainMenu()
        {
            var mainMenu = GameCanvas.Children
                .OfType<Border>()
                .FirstOrDefault(b => b.Name == "MainMenu");

            if (mainMenu != null)
            {
                GameCanvas.Children.Remove(mainMenu);
            }
            isInMainMenu = false;
        }

        private void ReturnToMainMenu()
        {
            // Останавливаем игру
            gameTimer.Stop();
            gameState.StopInfiniteMode();

            // Очищаем игровые объекты
            bulletManager.ClearAllBullets();
            enemyManager.ClearEnemies();
            shieldManager.ClearShields();

            // Сбрасываем состояние
            player.Reset();
            gameState.ResetGame();

            // Показываем главное меню
            CreateMainMenu();
            isInMainMenu = true;
        }


        private void StartCampaign()
        {
            // Запуск обычной кампании
            gameManager.ResetGame();
            gameTimer.Start();
        }

        private void StartInfiniteMode()
        {
            // Запуск бесконечного режима
            gameManager.StartInfiniteMode();
            gameTimer.Start();
        }

        private void OnResetLives()
        {
            player.Reset(); // Используем метод Reset вместо прямого присваивания
            uiManager.UpdateLives(player.Lives);
        }

        private void OnGameOver()
        {
            soundManager.PlaySound("game_over");

            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Создаем взрывы при Game Over
                var rnd = new Random();
                for (int i = 0; i < 5; i++)
                {
                    double delaySeconds = i * 0.2;
                    DispatcherTimer explosionTimer = new DispatcherTimer()
                    {
                        Interval = TimeSpan.FromSeconds(delaySeconds)
                    };
                    explosionTimer.Tick += (s, e) =>
                    {
                        animationManager.CreateExplosion(
                            rnd.NextDouble() * GameCanvas.ActualWidth,
                            rnd.NextDouble() * GameCanvas.ActualHeight,
                            40
                        );
                        explosionTimer.Stop();
                    };
                    explosionTimer.Start();
                }

                // Показываем сообщение после взрывов
                DispatcherTimer messageTimer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromSeconds(1.0)
                };
                messageTimer.Tick += (s, e) =>
                {
                    messageTimer.Stop();
                    MessageBox.Show($"Game over! Score: {gameState.Score}", "Game Over",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // ВОЗВРАТ В ГЛАВНОЕ МЕНЮ ПОСЛЕ ПРОИГРЫША
                    ReturnToMainMenu();
                };
                messageTimer.Start();
            }));
        }

        private void OnGameWon()
        {
            soundManager.PlaySound("win");

            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Праздничные взрывы при победе
                var rnd = new Random();
                for (int i = 0; i < 10; i++)
                {
                    double delaySeconds = i * 0.15;
                    DispatcherTimer explosionTimer = new DispatcherTimer()
                    {
                        Interval = TimeSpan.FromSeconds(delaySeconds)
                    };
                    explosionTimer.Tick += (s, e) =>
                    {
                        animationManager.CreateExplosion(
                            rnd.NextDouble() * GameCanvas.ActualWidth,
                            rnd.NextDouble() * GameCanvas.ActualHeight,
                            50
                        );
                        explosionTimer.Stop();
                    };
                    explosionTimer.Start();
                }

                // Показываем сообщение после взрывов
                DispatcherTimer messageTimer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromSeconds(1.5)
                };
                messageTimer.Tick += (s, e) =>
                {
                    messageTimer.Stop();
                    MessageBox.Show($"Поздравляем! Вы победили всех противников! Итоговый счёт: {gameState.Score}",
                        "Победа", MessageBoxButton.OK, MessageBoxImage.Information);

                    // ВОЗВРАТ В ГЛАВНОЕ МЕНЮ ПОСЛЕ ПОБЕДЫ
                    ReturnToMainMenu();
                };
                messageTimer.Start();
            }));
        }

        private void ResetGame()
        {
            gameManager.ResetGame();
        }

        private void InitializeGame()
        {

            uiManager.UpdateScore(0);
            uiManager.UpdateLives(player.Lives);
            uiManager.UpdateLevel(1);


            Loaded += (s, e) =>
            {
                pauseMenuManager.UpdatePosition(GameCanvas.ActualWidth, GameCanvas.ActualHeight);
                uiManager.UpdateBossHealthBarPosition(GameCanvas.ActualWidth);

                player.SetPosition((GameCanvas.ActualWidth - player.Visual.Width) / 2,
                                 GameCanvas.ActualHeight - player.Visual.Height - 20);

                shieldManager.CreateShields();
                gameManager.SpawnEnemiesForLevel(1);
            };


            SizeChanged += (s, e) =>
            {
                pauseMenuManager.UpdatePosition(GameCanvas.ActualWidth, GameCanvas.ActualHeight);
                uiManager.UpdateBossHealthBarPosition(GameCanvas.ActualWidth);
            };
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (gameState.IsPaused || isInMainMenu) return; // НЕ ОБНОВЛЯЕМ ИГРУ В ГЛАВНОМ МЕНЮ
            gameManager.UpdateGame();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isInMainMenu) return; // ИГНОРИРУЕМ УПРАВЛЕНИЕ В ГЛАВНОМ МЕНЮ

            if (e.Key == Key.Escape || e.Key == Key.P)
            {
                pauseMenuManager.TogglePause();
                return;
            }

            if (e.Key == Key.M)
            {
                soundManager.ToggleMute();
                return;
            }

            if (gameState.IsPaused) return;
            player.HandleKeyDown(e.Key);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (isInMainMenu || gameState.IsPaused) return;
            player.HandleKeyUp(e.Key);
        }
    }
}
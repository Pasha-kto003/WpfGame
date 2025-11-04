using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace WpfGame
{
    public class PauseMenuManager
    {
        private Border pauseMenu;
        private Button resumeButton;
        private Button exitButton;
        private TextBlock pauseTitle;
        private Canvas gameCanvas;
        private GameStateManager gameState;
        private Button soundButton;
        private SoundManager soundManager;

        private Slider volumeSlider; // ДОБАВЛЕНО: слайдер громкости
        private TextBlock volumeText; // ДОБАВЛЕНО: текст громкости

        public bool IsPaused => gameState.IsPaused;

        public PauseMenuManager(Canvas canvas, GameStateManager gameStateManager, SoundManager soundManager)
        {
            gameCanvas = canvas;
            gameState = gameStateManager;
            this.soundManager = soundManager;
            CreatePauseMenu();
        }

        private void CreatePauseMenu()
        {
            pauseMenu = new Border()
            {
                Width = 300,
                Height = 250, // УВЕЛИЧЕНО: для размещения слайдера
                Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 40)),
                BorderBrush = Brushes.Cyan,
                BorderThickness = new Thickness(3),
                CornerRadius = new CornerRadius(15),
                Visibility = Visibility.Collapsed
            };
            gameCanvas.Children.Add(pauseMenu);

            pauseTitle = new TextBlock()
            {
                Text = "ПАУЗА",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Cyan,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 20)
            };

            volumeText = new TextBlock()
            {
                Text = "Громкость:",
                FontSize = 12,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 2)
            };

            soundButton = new Button()
            {
                Content = "Звук: Вкл",
                Width = 120,
                Height = 35,
                FontSize = 14,
                Background = new SolidColorBrush(Color.FromRgb(100, 100, 200)),
                Foreground = Brushes.White,
                BorderBrush = Brushes.LightBlue,
                Margin = new Thickness(0, 0, 0, 10)
            };
            soundButton.Click += SoundButton_Click;

            resumeButton = new Button()
            {
                Content = "Продолжить",
                Width = 120,
                Height = 35,
                FontSize = 14,
                Background = new SolidColorBrush(Color.FromRgb(0, 100, 200)),
                Foreground = Brushes.White,
                BorderBrush = Brushes.LightBlue,
                Margin = new Thickness(0, 0, 0, 10)
            };
            resumeButton.Click += ResumeButton_Click;

            exitButton = new Button()
            {
                Content = "Выйти из игры",
                Width = 120,
                Height = 35,
                FontSize = 14,
                Background = new SolidColorBrush(Color.FromRgb(200, 60, 60)),
                Foreground = Brushes.White,
                BorderBrush = Brushes.LightCoral
            };
            exitButton.Click += ExitButton_Click;

            var stackPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            stackPanel.Children.Add(pauseTitle);
            stackPanel.Children.Add(resumeButton);
            stackPanel.Children.Add(exitButton);
            stackPanel.Children.Add(soundButton);

            pauseMenu.Child = stackPanel;
        }

        private void SoundButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.ToggleMute();
            soundButton.Content = soundManager.IsMuted ? "Звук: Выкл" : "Звук: Вкл";
        }

        public void UpdatePosition(double canvasWidth, double canvasHeight)
        {
            if (pauseMenu != null && canvasWidth > 0 && canvasHeight > 0)
            {
                Canvas.SetLeft(pauseMenu, (canvasWidth - pauseMenu.Width) / 2);
                Canvas.SetTop(pauseMenu, (canvasHeight - pauseMenu.Height) / 2);
            }
        }

        public void ShowPauseMenu()
        {
            gameState.IsPaused = true;
            UpdatePosition(gameCanvas.ActualWidth, gameCanvas.ActualHeight);
            pauseMenu.Visibility = Visibility.Visible;
            pauseMenu.Opacity = 0;

            // Обновляем состояние кнопки звука при показе меню
            soundButton.Content = soundManager.IsMuted ? "Звук: Выкл" : "Звук: Вкл";

            var animation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
            pauseMenu.BeginAnimation(Border.OpacityProperty, animation);
        }

        public void HidePauseMenu()
        {
            var animation = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.2));
            animation.Completed += (s, e) =>
            {
                pauseMenu.Visibility = Visibility.Collapsed;
                gameState.IsPaused = false;
            };
            pauseMenu.BeginAnimation(Border.OpacityProperty, animation);
        }

        public void TogglePause()
        {
            if (gameState.IsPaused)
            {
                HidePauseMenu();
            }
            else
            {
                ShowPauseMenu();
            }
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            HidePauseMenu();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из игры?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
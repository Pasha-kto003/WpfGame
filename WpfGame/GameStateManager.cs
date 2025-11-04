using System;
using System.Windows.Threading;

namespace WpfGame
{
    public class GameStateManager
    {
        public int Score { get; set; }
        public int Level { get; set; } = 1;
        public bool IsPaused { get; set; }
        public bool IsGameOver { get; private set; }
        public bool IsGameWon { get; private set; }
        public bool IsInfiniteMode { get; set; }
        public TimeSpan InfiniteModeTime { get; set; }

        // НОВОЕ: Событие для сброса жизней
        public event Action ResetLives;

        private DispatcherTimer gameTimer;
        private DispatcherTimer infiniteModeTimer;

        public GameStateManager(DispatcherTimer timer)
        {
            gameTimer = timer;
            infiniteModeTimer = new DispatcherTimer();
            infiniteModeTimer.Interval = TimeSpan.FromSeconds(1);
            infiniteModeTimer.Tick += (s, e) => InfiniteModeTime = InfiniteModeTime.Add(TimeSpan.FromSeconds(1));
        }

        public void IncreaseScore(int points)
        {
            Score += points;
        }

        public void NextLevel()
        {
            Level++;
        }

        public void ResetGame()
        {
            Score = 0;
            Level = 1;
            IsGameOver = false;
            IsGameWon = false;
            IsPaused = false;
            IsInfiniteMode = false; // НОВОЕ: сбрасываем флаг бесконечного режима
            InfiniteModeTime = TimeSpan.Zero;
            // ВЫЗЫВАЕМ СОБЫТИЕ СБРОСА ЖИЗНЕЙ
            ResetLives?.Invoke();
        }

        public void StartInfiniteMode() // НОВЫЙ МЕТОД: запуск бесконечного режима
        {
            IsInfiniteMode = true;
            InfiniteModeTime = TimeSpan.Zero;
            infiniteModeTimer.Start();
        }

        public void StopInfiniteMode() // НОВЫЙ МЕТОД: остановка бесконечного режима
        {
            IsInfiniteMode = false;
            infiniteModeTimer.Stop();
        }

        public void TriggerGameOver()
        {
            IsGameOver = true;
            gameTimer.Stop();
            infiniteModeTimer.Stop();
            GameOver?.Invoke();
        }

        public void TriggerWinGame()
        {
            IsGameWon = true;
            gameTimer.Stop();
            infiniteModeTimer.Stop();
            GameWon?.Invoke();
        }

        public void StartGame()
        {
            IsGameOver = false;
            IsGameWon = false;
            gameTimer.Start();
        }

        public bool ShouldSpawnBoss()
        {
            return Level == 4 || (IsInfiniteMode && (int)InfiniteModeTime.TotalMinutes % 3 == 0 && InfiniteModeTime.TotalSeconds > 10);
        }

        public bool IsBossLevel()
        {
            return Level >= 4;
        }

        public bool ShouldSpawnInfiniteBoss() // НОВЫЙ МЕТОД: проверка появления босса в бесконечном режиме
        {
            return IsInfiniteMode && (int)InfiniteModeTime.TotalMinutes % 3 == 0 &&
                   InfiniteModeTime.TotalSeconds % 60 < 30; // Босс появляется на 30 секунд каждые 3 минуты
        }

        // Существующие события
        public event Action GameOver;
        public event Action GameWon;
    }
}
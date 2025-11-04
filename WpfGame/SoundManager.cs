using System;
using System.Media;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace WpfGame
{
    public class SoundManager
    {
        private Dictionary<string, SoundPlayer> soundPlayers = new Dictionary<string, SoundPlayer>();
        private bool isMuted = false;
        private double volume = 100;

        public bool IsMuted => isMuted;

        public SoundManager()
        {
            LoadSounds();
        }

        private void LoadSounds()
        {
            // Загружаем все звуки в формате WAV
            TryLoadSound("shoot", "Sounds/shoot.wav");
            TryLoadSound("enemy_shoot", "Sounds/enemy_shoot.wav");
            TryLoadSound("explosion", "Sounds/explosion.wav");
            TryLoadSound("boss_hit", "Sounds/boss_hit.wav");
            TryLoadSound("game_over", "Sounds/game_over.wav");
            TryLoadSound("win", "Sounds/win.wav");
        }

        private void TryLoadSound(string soundName, string soundPath)
        {
            try
            {
                // Получаем путь к файлу относительно исполняемого файла
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, soundPath);

                if (File.Exists(fullPath))
                {
                    var soundPlayer = new SoundPlayer(fullPath);
                    soundPlayer.LoadAsync(); // Асинхронная загрузка
                    soundPlayers[soundName] = soundPlayer;
                    System.Diagnostics.Debug.WriteLine($"Successfully loaded sound: {soundName}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Sound file not found: {fullPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load sound {soundName}: {ex.Message}");
            }
        }

        public void PlaySound(string soundName)
        {
            if (isMuted || !soundPlayers.ContainsKey(soundName)) return;

            try
            {
                soundPlayers[soundName].Play();
                System.Diagnostics.Debug.WriteLine($"Playing sound: {soundName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to play sound {soundName}: {ex.Message}");
            }
        }

        public void PlaySoundLoop(string soundName)
        {
            if (isMuted || !soundPlayers.ContainsKey(soundName)) return;

            try
            {
                soundPlayers[soundName].PlayLooping();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to play loop sound {soundName}: {ex.Message}");
            }
        }

        public void StopSound(string soundName)
        {
            if (soundPlayers.ContainsKey(soundName))
            {
                soundPlayers[soundName].Stop();
            }
        }

        public void StopAllSounds()
        {
            foreach (var player in soundPlayers.Values)
            {
                player.Stop();
            }
        }

        // SoundPlayer не поддерживает регулировку громкости, но оставим методы для совместимости
        public void SetVolume(double newVolume)
        {
            volume = Math.Max(0, Math.Min(1, newVolume));
            // SoundPlayer не поддерживает регулировку громкости
        }

        public double GetVolume()
        {
            return volume;
        }

        public void Mute()
        {
            isMuted = true;
            StopAllSounds();
        }

        public void Unmute()
        {
            isMuted = false;
        }

        public void ToggleMute()
        {
            isMuted = !isMuted;
            if (isMuted)
            {
                StopAllSounds();
            }
        }
    }
}
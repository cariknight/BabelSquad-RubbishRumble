using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RubbishRumble.Services;
using System.Windows.Input;

namespace RubbishRumble.ViewModels
{
    public class GameOverViewModel : BindableObject
    {
        private int _totalScore;
        private int _earnedCoins;

        public int TotalScore
        {
            get => _totalScore;
            set
            {
                _totalScore = value;
                OnPropertyChanged();
            }
        }

        public int EarnedCoins
        {
            get => _earnedCoins;
            set
            {
                _earnedCoins = value;
                OnPropertyChanged();
            }
        }

        public ICommand ExitCommand { get; }

        public GameOverViewModel()
        {
            // TOFIX: just tested values
            TotalScore = 200;
            EarnedCoins = 100;
            //TOFIX: add music for game over _ = SettingsService.Instance.PlaySfxAsync("gameover.mp3");
        }

    }
}
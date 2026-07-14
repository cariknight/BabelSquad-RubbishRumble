using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RubbishRumble.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        public ICommand PauseGameCommand { get; }

        public GameViewModel()
        {
            PauseGameCommand = new Command(OnPauseGameExecuted);
        }

        private void OnPauseGameExecuted()
        {
        }
    }
}

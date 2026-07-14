using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RubbishRumble.Services
{
    public class GameService
    {
        //Start game
        //End game
        //Increase score
        //Lose life
        //Spawn trash
        //Activate power-ups
        //Increase difficulty

        public int Score { get; private set; }
        public int Lives { get; private set; }
        public int Coins { get; private set; }
        public bool IsGGameOver => Lives <= 0;

        public void StartGame()
        {
            Score = 0;
            Coins = 0;
            Lives = 3;
        }

        public void AddScore(int amount)
        {
            Score += amount;
        }

        public void AddCoins(int amount)
        {
            Coins += amount;
        }

        public void LoseLife()
        {
            Lives--;
            if (Lives <= 0)
            {
                EndGame();
            }
        }

        public void Endame()
        {

        }
    }
}

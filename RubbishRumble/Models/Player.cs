using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubbishRumble.Models
{
    public class Player
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } = 1;
        public int Coins { get; set; }
        public int HighestScore { get; set; }
        public int TotalGamesPlayed { get; set; }
        public bool ReceivedBeginnerPack { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace RubbishRumble.Models
{
    public class GameSession
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int FinalScore { get; set; }
        public int CoinsEarned { get; set; }
        public DateTime PlayedAt { get; set; }
    }
}

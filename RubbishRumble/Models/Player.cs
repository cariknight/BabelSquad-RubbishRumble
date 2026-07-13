using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubbishRumble.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int Coins { get; set; }

        public int HighestScore { get; set; }
    }
}

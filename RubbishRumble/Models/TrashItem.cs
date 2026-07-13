using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubbishRumble.Models
{
    public class TrashItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Rarity { get; set; }
        public int CoinReward { get; set; }
        public int ScoreValue { get; set; }
        
    }
}

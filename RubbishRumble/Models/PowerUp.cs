using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubbishRumble.Models
{
    public class PowerUp
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EffectType { get; set; }
        public string Description { get; set; }
        public int DurationSeconds { get; set; }
        public double ScoreMultiplier { get; set; }
        public double SpeedMultiplier { gget; set; }
        public int Quantity { get; set; }
    }
}

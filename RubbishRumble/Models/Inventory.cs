using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubbishRumble.Models
{
    public class Inventory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string PlayerId { get; set; }
        public string PowerUpName { get; set; }
        public int Quantity { get; set; }
    }
}

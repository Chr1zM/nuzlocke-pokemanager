using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeManager.PokeManagement.Saving
{
    public class SaveData
    {
        public Player Player1 { get; set; } = new();
        public Player Player2 { get; set; } = new();
        public List<LocationModel> Locations { get; set; } = [];
    }
}

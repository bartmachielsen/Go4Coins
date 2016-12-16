using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Maps;
using Newtonsoft.Json;
using UWPEindopdracht.GPSConnections;

namespace UWPEindopdracht.Multiplayer
{
    public class User
    {
        [JsonIgnore]
        public string id { get; set; }
        public string Name { get; set; }
        public GCoordinate Location { get; set; }
        public DateTime LastSynced { get; set; }
        [JsonIgnore]
        public LastState LastState;
        
        [JsonIgnore]
        public MapIcon Icon { get; set; }

        public int Coins { get; set; } = 0;

        public List<string> Rewards { get; set; } = new List<string>();
        public User(string id, string name, GCoordinate location)
        {
            this.id = id;
            Name = name;
            this.Location = location;
        }

    }

    public enum LastState
    {
        Online, Offline, Updated
    }
}

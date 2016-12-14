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
        public GCoordinate location { get; set; }
        public DateTime lastSynced { get; set; }
        [JsonIgnore]
        public bool Self { get; set; }
        [JsonIgnore]
        public MapIcon Icon { get; set; }

        public User(string id, string name, GCoordinate location)
        {
            this.id = id;
            Name = name;
            this.location = location;
        }

    }
}

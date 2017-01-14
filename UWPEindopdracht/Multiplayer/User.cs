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
        public MapIcon Icon { get; set; }
        public List<string> AcceptedDuels = new List<string>();
        public DuelRequest Duel { get; set; }

        public int Coins { get; set; } = 0;

        public List<string> Rewards { get; set; } = new List<string>();
        public List<string> Chests { get; set; } = new List<string>();
        public User(string id, string name, GCoordinate location)
        {
            this.id = id;
            Name = name;
            this.Location = location;
        }

        public List<RewardChest> getChests()
        {
            List<RewardChest> chests = new List<RewardChest>();
            foreach (var chest in Chests)
            {
                if (chest == typeof(BasicChest).Name)
                    chests.Add(new BasicChest());
                if (chest == typeof(AdvancedChest).Name)
                    chests.Add(new AdvancedChest());
                if (chest == typeof(LargeChest).Name)
                    chests.Add(new LargeChest());
            }
            return chests;
        }

        public bool IsAlive()
        {
            return !((DateTime.Now - LastSynced) >= TimeSpan.FromSeconds(MultiplayerData.ServerTimeOut*3));
        }

    }

    public class DuelRequest
    {
        public string MultiplayerID;
        public string UserID;
    }
    
}

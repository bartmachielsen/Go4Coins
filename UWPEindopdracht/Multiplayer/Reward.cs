using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;

namespace UWPEindopdracht.Multiplayer
{
    public enum RewardValue
    {
        Legendary=1, Epic=2, Rare=3, Normal=4
    }

    public class Reward
    {
        [JsonIgnore]
        public string id { get; set; }
        public string Name { get; set; }
        public string UnlockedImageLocation { get; set; }
        public string LockedImageLocation { get; set; }
        public string Description { get; set; }
        public string Categorie { get; set; }

        public int InInventory { get; set; } = 0;

        public string Image => InInventory > 0 ? UnlockedImageLocation : LockedImageLocation;

        public string NiceName => InInventory > 1 ? Name + $" [{InInventory}x]" : Name;

        public int CoinValue => (int)(1000*(1/(double)(int)Value));

        public RewardValue Value { get; set; }

        public SolidColorBrush RareColor
        {
            get
            {
              if((int)Value == 1)
                    return new SolidColorBrush(Colors.Orange);
                if ((int)Value == 2)
                    return new SolidColorBrush(Colors.Purple);
                if ((int)Value == 3)
                    return new SolidColorBrush(Colors.Blue);
                return new SolidColorBrush(Colors.Gray);
            }
        }

        public Reward(string _id, string name, string unlockedImageLocation, string lockedImageLocation, string description, string categorie, RewardValue value)
        {
            this.id = _id;
            Name = name;
            UnlockedImageLocation = unlockedImageLocation;
            LockedImageLocation = lockedImageLocation;
            Description = description;
            Categorie = categorie;
            Value = value;
        }
    }

    public class RewardChest
    {
        public string Name;
        private double _price;

        internal int[] Chance = new int[Enum.GetNames(typeof(RewardValue)).Length];

        private int _amount;

        public string ImageLocation { get; set; }
        public RewardChest(string name, double price, int amount)
        {
            Name = name;
            _price = price;
            this._amount = amount;
        }
       
        public List<Reward> GetRewards(List<Reward> rewards)
        {
            Random random = new Random();
            List<Reward> chosen = new List<Reward>();
            while (chosen.Count < _amount)
            {
                int previous = 0;
                
                var reversed = Enum.GetValues(typeof(RewardValue));
                Array.Reverse(reversed);
                int i = reversed.Length-1;
                foreach (RewardValue value in reversed)
                {
                    if (chosen.Count >= _amount)
                        continue;
                    List<Reward> sorted = rewards.FindAll(reward => reward.Value == value);
                    int picked = random.Next(100);
                    System.Diagnostics.Debug.WriteLine($"Must be between {previous} and {Chance[i]} and picked {picked}");
                    if (picked > previous && picked < Chance[i])
                    {
                        var pic = sorted.ElementAt(random.Next(sorted.Count));
                        chosen.Add(pic);
                    }
                    previous = Chance[i];
                    i--;
                }
            }
            return chosen;
        }
    }

    public class BasicChest : RewardChest
    {
        public BasicChest() : base("Basic Chest", 50.0, 3)
        {
            Chance = new int[]
            {
                100, 99, 90, 65
            };
        }
    }

    public class AdvancedChest : RewardChest
    {
        public AdvancedChest() : base("Advanced Chest", 100.0, 3)
        {
            Chance = new int[]
            {
                100, 90, 80, 50
            };
        }
    }

    public class LargeChest : RewardChest
    {
        public LargeChest() : base("Large Chest", 100.0, 8)
        {
            Chance = new int[]
            {
                100, 99, 90, 65
            };
        }
    }
   
}

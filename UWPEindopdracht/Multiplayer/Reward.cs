using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UWPEindopdracht.Multiplayer
{
    public enum RewardValue
    {
        Legendary, Epic, Rare, Normal
    }

    public class Reward
    {
        [JsonIgnore]
        public string id { get; set; }
        public string Name { get; set; }
        public string ImageLocation { get; set; }
        public string Description { get; set; }

        public RewardValue Value { get; set; }

        public Reward(string _id, string name, string imageLocation, string description, RewardValue value)
        {
            id = _id;
            Name = name;
            ImageLocation = imageLocation;
            Description = description;
            Value = value;
        }
    }

    class RewardChest
    {
        private string _name;
        private double _price;

        internal int[] Chance = new int[Enum.GetNames(typeof(RewardValue)).Length];

        private int _amount;

        public string ImageLocation { get; set; }
        public RewardChest(string name, double price, int amount)
        {
            _name = name;
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
                for(int i = Enum.GetValues(typeof(RewardValue)).Length; i >= 0; i++)
                {
                    var value = (RewardValue)Enum.GetValues(typeof(RewardValue)).GetValue(i);
                    List<Reward> sorted = rewards.FindAll(reward => reward.Value == value);
                    int picked = random.Next(100);
                    if (picked > previous && picked < Chance[i]) 
                        chosen.Add(sorted.ElementAt(random.Next(chosen.Count)));
                    previous = Chance[i];

                }
            }
            return chosen;
        }
    }

    static class ChestCollection
    {
        public static List<RewardChest> Chests = new List<RewardChest>()
        {
            new RewardChest("Basic Chest", 50.0, 3)
            {
                Chance = new int[]
                {
                    100, 99, 90, 65
                },
                ImageLocation = "LOCATION"
            },new RewardChest("Advanced Chest", 100.0, 3)
            {
                Chance = new int[]
                {
                    100, 90, 80, 50
                },
                ImageLocation = "LOCATION"
            },new RewardChest("Large Chest", 100.0, 8)
            {
                Chance = new int[]
                {
                    100, 99, 90, 65
                },
                ImageLocation = "LOCATION"
            }
        };
    }
}

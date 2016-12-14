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
        public string Name { get; set; }
        public double Price { get; set; }

        public double[] Changes = new double[Enum.GetNames(typeof(RewardValue)).Length];

        public RewardChest(string name, double price)
        {
            Name = name;
            Price = price;
        }
    }
}

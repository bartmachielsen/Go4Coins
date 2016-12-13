using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Maps;
using Windows.UI.Xaml.Media.Animation;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.Places;

namespace UWPEindopdracht
{
    

    internal abstract class Assignment
    {
        public int MaxDistance { get; set; }
        public int MinDistance { get; set; }
        public int? MaxSpeed { get; set; }
        public string Description { get; set; }
        public Place Target { get; set; }

        protected double TimeMultiplier { get; set; } = 1;

        protected DateTime start;

        public bool ShowPinPoint { get; set; } = true;
        public TimeSpan MaximumTime { get; set; }

        // TODO IMAGE FOR SHOWING WHEN ASSIGNMENT HAS BEEN ANNOUNCED!

        public virtual async Task FillTarget(List<Place> places, GCoordinate currentPosition)
        {
            Target = await PickTargetPlace(places, currentPosition);
            MaximumTime = TimeSpan.FromMinutes((await GPSHelper.calculateRouteBetween(currentPosition, Target.Location)).EstimatedDuration.TotalMinutes*TimeMultiplier);
            FillDescription();
            start = DateTime.Now;
            
        }

        public abstract void FillDescription();
        /// <summary>
        /// Select target random from a list with only items that are on a good distance from source
        /// </summary>
        /// <param name="places"></param>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        public async Task<Place> PickTargetPlace(List<Place> places, GCoordinate currentPosition)
        {
            foreach (var place in places)
            {
                if (place.Distance != null)
                    place.Distance =
                        (await GPSHelper.calculateRouteBetween(currentPosition, place.Location)).LengthInMeters;
            }

            places.RemoveAll((Place place) => place.Distance < MinDistance || place.Distance > MaxDistance || place.IsCity());
            if (places.Count == 0)
                return null;
            if (places.Count == 1)
                return places.ElementAt(0);
            Random random = new Random();
            return places.ElementAt(random.Next(places.Count));
        }

        /// <summary>
        /// Calculating the total reachable score with the current time
        /// </summary>
        /// <param name="currentTimeSpent"></param>
        /// <returns></returns>
        public double TotalScore(TimeSpan currentTimeSpent)
        {
            double distance = 0;
            double timebonus = 0;
            if (Target.Distance != null)
                distance = (double)Target.Distance;
            if (currentTimeSpent.TotalSeconds < MaximumTime.TotalSeconds)
                timebonus = MaximumTime.TotalSeconds - currentTimeSpent.TotalSeconds;

            return distance * 10 + timebonus * 15;
        }


        public async Task<string[]> GetRouteInformation(GCoordinate currentPoint)
        {
            MapRoute route = await GPSHelper.calculateRouteBetween(currentPoint, Target.Location);
            TimeSpan span = MaximumTime-DateTime.Now.Subtract(start);
        
            return new string[] {$"{span.Minutes}:{span.Seconds}", route.LengthInMeters/1000.0 + " km"};
        }
    }

    class MapAssignment : Assignment
    {
        public MapAssignment()
        {
            MaxDistance = 3000;
            MinDistance = 800;
            MaxSpeed = 40;
            TimeMultiplier = 0.9;
        }
        
        public override void FillDescription()
        {
            Description =
                "Walk to the marked point on the map, " +
                $"\n Bonus if reached within {MaximumTime.TotalMinutes} minutes." +
                $"\n total score could be {TotalScore(new TimeSpan())}!";
        }

    }


    class SearchAssignment : Assignment
    {
        public SearchAssignment()
        {
            MaximumTime = TimeSpan.FromHours(1);
            MaxSpeed = null;
            MaxDistance = 1000;
            MinDistance = 100;
            ShowPinPoint = false;
            TimeMultiplier = 1.5;
        }

       

        public override void FillDescription()
        {

            Description =
                "Search the given point!" +
                $"\n Name: {Target.Name}" +
                $"\n Estimated distance to the target {Target.Distance}!" +
                $"\n Bonus if reached within {MaximumTime.TotalMinutes} minutes." +
                $"\n Maximum score is {TotalScore(new TimeSpan())}";
        }
    }
}

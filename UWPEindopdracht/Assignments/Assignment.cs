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
    public abstract class Assignment
    {
        protected int MaxDistance { get; set; }
        protected int MinDistance { get; set; }
        protected int? MaxSpeed { get; set; }
        public string Description { get; set; }
        public Place[] Target { get; private set; }

        protected double TimeMultiplier { private get; set; } = 1;

        private DateTime _start;

        public bool ShowPinPoint { get; protected set; } = true;
        protected TimeSpan MaximumTime { get; set; }
        public bool ShowPicture { get; set; } = true;

        // TODO IMAGE FOR SHOWING WHEN ASSIGNMENT HAS BEEN ANNOUNCED!

        public virtual async Task FillTarget(List<Place> places, GCoordinate currentPosition)
        {
            Target = await PickTargetPlace(places, currentPosition);
            MaximumTime = TimeSpan.FromMinutes((await GPSHelper.calculateRouteBetween(currentPosition, Target[0].Location)).EstimatedDuration.TotalMinutes*TimeMultiplier);
            FillDescription();
            _start = DateTime.Now;
            
        }

        public abstract void FillDescription();
        /// <summary>
        /// Select target random from a list with only items that are on a good distance from source
        /// </summary>
        /// <param name="places"></param>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        public async Task<Place[]> PickTargetPlace(List<Place> places, GCoordinate currentPosition)
        {

            /**foreach (var place in places)
            {
                if (place.Distance == null)
                {
                    var route = await GPSHelper.calculateRouteBetween(currentPosition, place.Location);
                    if(route != null)
                        place.Distance = route.LengthInMeters;
                }
            }
            
            places.RemoveAll((Place place) => place.Distance == null || place.Distance < MinDistance || place.Distance > MaxDistance || place.IsCity());
            if (places.Count == 0)
                return null;
            if (places.Count == 1)
                return places.ElementAt(0);
            Random random = new Random();
            System.Diagnostics.Debug.WriteLine($"Filtered the places to {places.Count} Places!");
            return places.ElementAt(random.Next(places.Count));
            **/
            Random random = new Random();
            List<Place> removed = new List<Place>(places);
            while (removed.Count > 0)
            {
                Place place = removed.ElementAt(random.Next(removed.Count));
                removed.Remove(place);
                var route = await GPSHelper.calculateRouteBetween(currentPosition, place.Location);
                if (route != null)
                    place.Distance = route.LengthInMeters;
                if (place.Distance >= MinDistance && place.Distance <= MaxDistance)
                {
                    return new[] { place};
                }
            }
            throw new NoTargetAvailable();
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
            if (Target[0].Distance != null)
                distance = (double)Target[0].Distance;
            if (currentTimeSpent.TotalSeconds < MaximumTime.TotalSeconds)
                timebonus = MaximumTime.TotalSeconds - currentTimeSpent.TotalSeconds;

            return distance * 10 + timebonus * 15;
        }


        public async Task<string[]> GetRouteInformation(GCoordinate currentPoint, bool wantRoute = true)
        {
            TimeSpan span = MaximumTime - DateTime.Now.Subtract(_start);
            if (!wantRoute)
            {
                string hours = "";
                if (span.Hours > 0)
                    hours = span.Hours + ":";
                if (hours.Length < 3 && span.Hours > 0)
                    hours = "0" + hours;
                string minutes = span.Minutes + "";
                if (minutes.Length < 2)
                    minutes = "0" + minutes;
                string seconds = span.Seconds + "";
                if (seconds.Length < 2)
                    seconds = "0" + seconds;
                return new string[] {$"{hours}{minutes}:{seconds}"};
            }
            MapRoute route = await GPSHelper.calculateRouteBetween(currentPoint, Target[0].Location);
            return new string[] {$"", route.LengthInMeters/1000.0 + " km"};
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
                $"\n Name: {Target[0].Name}" +
                $"\n Estimated distance to the target {Target[0].Distance}!" +
                $"\n Bonus if reached within {MaximumTime.TotalMinutes} minutes." +
                $"\n Maximum score is {TotalScore(new TimeSpan())}";
        }
    }

    class NoTargetAvailable : Exception
    {
        public NoTargetAvailable() : base("No available targets!")
        {
        }
    }
}

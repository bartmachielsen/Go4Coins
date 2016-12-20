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
        private SpeedChecker _speedControl = new SpeedChecker();
        protected int MinDistance { get; set; }
        public int NeededDistance { get; set; }
        protected int? MaxSpeed { get; set; }
        public Place[] Target { get; private set; }
        public Place CurrentLocation { get; set; }
        private List<Place> _reachedTargets = new List<Place>();
        protected double TimeMultiplier { private get; set; } = 1;

        private DateTime _start;

        public bool ShowPinPoint { get; protected set; } = true;
        protected TimeSpan MaximumTime { get; set; }
        public bool ShowPicture { get; set; } = true;
        public bool Skippable { get; set; } = true;

        // TODO IMAGE FOR SHOWING WHEN ASSIGNMENT HAS BEEN ANNOUNCED!

        public virtual async Task FillTarget(List<Place> places, GCoordinate currentPosition)
        {
            Target = await PickTargetPlace(places, currentPosition);
            var route = await GPSHelper.calculateRouteBetween(currentPosition, Target[0].Location);
            if(route != null)
                MaximumTime = TimeSpan.FromMinutes(route.EstimatedDuration.TotalMinutes*TimeMultiplier);  
        }

        public abstract string GetDescription();
        /// <summary>
        /// Select target random from a list with only items that are on a good distance from source
        /// </summary>
        /// <param name="places"></param>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        public async Task<Place[]> PickTargetPlace(List<Place> places, GCoordinate currentPosition)
        {
            Random random = new Random();
            List<Place> removed = new List<Place>(places);
            while (removed.Count > 0)
            {
                Place place = removed.ElementAt(random.Next(removed.Count));
                removed.Remove(place);
                if (place.IsCity() || place.Name == await GPSHelper.GetCityName(place.Location))
                    continue;
                var route = await GPSHelper.calculateRouteBetween(currentPosition, place.Location);
                if (route != null)
                    place.Distance = route.LengthInMeters;
                if (place.Distance >= MinDistance && place.Distance <= MaxDistance)
                {
                    return new[] {place};
                }
            }
            throw new NoTargetAvailable();
        }

        /// <summary>
        /// Calculating the total reachable score with the current time
        /// </summary>
        /// <param name="currentTimeSpent"></param>
        /// <returns></returns>
        public int TotalScore(TimeSpan currentTimeSpent)
        {
            double distance = 0;
            double timebonus = 0;
            if (Target[0].Distance != null)
                distance = (double)Target[0].Distance;
            if (currentTimeSpent.TotalSeconds < MaximumTime.TotalSeconds)
                timebonus = MaximumTime.TotalSeconds - currentTimeSpent.TotalSeconds;

            return (int)(distance/4.0 + timebonus*4);
        }

        public TimeSpan GetSpentTime()
        {
         return DateTime.Now - _start;
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
            if (route != null)
            {
                return new string[] {$"", route.LengthInMeters / 1000.0 + " km"};
            }
            return new string[] { $"", "Unreachable!" };
        }


        private async void CheckTargetsForReached()
        {
            Func<Place, Task> reachedFunction = null;
            Func<Place, Task> leavedFunction = null;

            reachedFunction = async (place) =>
            {
                if (CurrentLocation == null || CurrentLocation != place)
                {
                    System.Diagnostics.Debug.WriteLine("Reached place! " + place.Name);
                    CurrentLocation = place;
                    await GPSHelper.PlaceLeaved(leavedFunction, place, NeededDistance);
                }
            };
            leavedFunction = async (place) =>
            {
                if (CurrentLocation != null || CurrentLocation == place)
                {
                    System.Diagnostics.Debug.WriteLine("Left place! " + place.Name);
                    CurrentLocation = null;
                    await GPSHelper.PlaceEntered(reachedFunction, place, NeededDistance);
                }
            };
            foreach (var target in Target)
            {
                await GPSHelper.PlaceEntered(reachedFunction, target, NeededDistance);
            }
        }

        public void StartAssignment()
        {
            _start = DateTime.Now;
            foreach (var target in Target)
            {
                target.visited = false;
            }
            CheckTargetsForReached();
        }

        public bool RegisterTarget(Place place)
        {
            if (_reachedTargets.Contains(place)) return false;
            _reachedTargets.Add(place);
            return true;
        }

        public bool AssignmentFinished()
        {
            return Target.All(target => _reachedTargets.Contains(target));
        }

        public void LocationChanged(GCoordinate coordinate)
        {
            if(MaxSpeed != null)
                _speedControl.MaxSpeed = MaxSpeed.Value;
            _speedControl.RegisterLocationChange(coordinate);
        }
    }

    class MapAssignment : Assignment
    {
        public MapAssignment()
        {

            MaxDistance = 3000;
            MinDistance = 800;
            NeededDistance = 30;
            MaxSpeed = 40;
            TimeMultiplier = 0.6;
        }
        
        public override string GetDescription()
        {
            return
                "Walk to the marked point on the map!\n" +
                $"\nYou'll get a bonus if the point is reached within {MaximumTime.TotalMinutes} minutes!" +
                $"\nYour total score can be {TotalScore(new TimeSpan())}!";
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
            NeededDistance = 50;
            ShowPinPoint = false;
            TimeMultiplier = 1.5;
        }

       

        public override string GetDescription()
        {

            return
                "Search the point given below!\n" +
                $"\nName: {Target[0].Name}" +
                $"\nEstimated distance to the target: {Target[0].Distance}!" +
                $"\nYou'll get a bonus if the point is reached within {MaximumTime.TotalMinutes} minutes!" +
                $"\nYour total score can be {TotalScore(new TimeSpan())}!";
        }
    }

    class NoTargetAvailable : Exception
    {
        public NoTargetAvailable() : base("No available targets!")
        {
        }
    }
}

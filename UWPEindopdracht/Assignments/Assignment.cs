using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Maps;
using Windows.UI.Xaml.Media.Animation;
using UWPEindopdracht.DataConnections;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.Places;

namespace UWPEindopdracht
{
    public abstract class Assignment
    {
        protected int MaxDistance { get; set; }
        public bool PictureNeeded { get; set; } = false;
        private SpeedChecker _speedControl = new SpeedChecker();
        protected int MinDistance { get; set; }
        public int NeededDistance { get; set; }
        protected int? MaxSpeed { get; set; }
        public Place[] Targets { get; private set; }
        public Place CurrentLocation { get; set; }
        private List<Place> _reachedTargets = new List<Place>();
        protected double TimeMultiplier { private get; set; } = 1;
        private int errorsWithLoading = 0;
        private DateTime _start;

        public bool ShowPinPoint { get; protected set; } = true;
        protected TimeSpan MaximumTime { get; set; }
        public bool ShowPicture { get; set; } = true;
        public bool Skippable { get; set; } = true;

        // TODO IMAGE FOR SHOWING WHEN ASSIGNMENT HAS BEEN ANNOUNCED!

        public virtual async Task FillTarget(List<Place> places, GCoordinate currentPosition)
        {
            Targets = await PickTargetPlace(places, currentPosition);
            var route = await GPSHelper.calculateRouteBetween(currentPosition, Targets[0].Location);
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
        public virtual async Task<Place[]> PickTargetPlace(List<Place> places, GCoordinate currentPosition)
        {
            Random random = new Random();
            List<Place> removed = new List<Place>(places);
            while (removed.Count > 0)
            {
                Place place = removed.ElementAt(random.Next(removed.Count));
                removed.Remove(place);
                if (place.IsCity() || place.Name == await GPSHelper.GetCityName(place.Location))
                    continue;
                var result = await GPSHelper.getRouteResult(currentPosition, place.Location);
                if (result.Status == MapRouteFinderStatus.NoRouteFoundWithGivenOptions)
                {
                    if (errorsWithLoading >= 3)
                    {
                        errorsWithLoading = 0;
                        throw new CantCalculateRouteException();
                    }
                    errorsWithLoading += 1;
                    continue;
                }
                

                var route = result.Route;
                
                place.Distance = route.LengthInMeters;
                if (PictureNeeded)
                    if ((await ImageLoader.GetBestUrlFromPlace(place)) == null)
                        continue;
                    
                
                
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
            if (Targets[0].Distance != null)
                distance = (double)Targets[0].Distance;
            if (currentTimeSpent.TotalSeconds < MaximumTime.TotalSeconds)
                timebonus = MaximumTime.TotalSeconds - currentTimeSpent.TotalSeconds;

            return (int)(distance/4.0 + timebonus*4);
        }

        public TimeSpan GetSpentTime()
        {
         return DateTime.Now - _start;
        }

        public virtual async Task<string> GetDistanceText(GCoordinate currentPoint)
        {
            if (Targets == null) return "";   
            MapRoute route = await GPSHelper.calculateRouteBetween(currentPoint, Targets[0].Location);
            if (route != null)
            {
                return route.LengthInMeters / 1000.0 + " km";
            }
            return "Unreachable!";
        }
        public string GetTimeText()
        {
            if (Targets == null)
                return "";
            TimeSpan span = MaximumTime - DateTime.Now.Subtract(_start);
            string hours = "";
            if (span.Hours > 0)
                hours = Math.Abs(span.Hours) + ":";
            if (hours.Length < 3 && span.Hours > 0)
                hours = "0" + hours;
            string minutes = Math.Abs(span.Minutes) + "";
            if (minutes.Length < 2)
                minutes = "0" + minutes;
            string seconds = Math.Abs(span.Seconds) + "";
            if (seconds.Length < 2)
                seconds = "0" + seconds;
            if (span <= TimeSpan.Zero)
                return null;
            return $"{hours}{minutes}:{seconds}";
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
            foreach (var target in Targets)
            {
                await GPSHelper.PlaceEntered(reachedFunction, target, NeededDistance);
            }
        }

        public void StartAssignment()
        {
            _start = DateTime.Now;
            foreach (var target in Targets)
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
            return Targets.All(target => _reachedTargets.Contains(target));
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
            PictureNeeded = true;
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
                $"\nName: {Targets[0].Name}" +
                $"\nEstimated distance to the target: {Targets[0].Distance} meters!" +
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

    class CantCalculateRouteException : Exception
    {
        
    }
}

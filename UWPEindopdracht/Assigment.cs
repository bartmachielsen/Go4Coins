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

    public class AssigmentHelper
    {
       

    }
    abstract class Assigment
    {
        public int maxDistance { get; set; }
        public int minDistance { get; set; }
        public int? maxSpeed { get; set; }
        public string description { get; set; }
        public Place target { get; set; }
        public TimeSpan maximumTime { get; set; }


        public async void fillTarget(Place[] places, GCoordinate currentPosition)
        {
            target = await pickTargetPlace(places, currentPosition);
            maximumTime = (await GPSHelper.calculateRouteBetween(currentPosition, target.Location)).EstimatedDuration;
            fillDescription();
        }

        public abstract void fillDescription();
        /// <summary>
        /// Select target random from a list with only items that are on a good distance from source
        /// </summary>
        /// <param name="places"></param>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        public async Task<Place> pickTargetPlace(Place[] places, GCoordinate currentPosition)
        {
            List<Place> sortedPlaces = new List<Place>(places);
            foreach (var place in sortedPlaces)
                place.Distance = (await GPSHelper.calculateRouteBetween(currentPosition, place.Location)).LengthInMeters;

            sortedPlaces.RemoveAll((Place place) => place.Distance < minDistance || place.Distance > maxDistance || place.isCity());
            if (sortedPlaces.Count == 0)
                return null;
            if (sortedPlaces.Count == 1)
                return sortedPlaces.ElementAt(0);
            Random random = new Random();
            return sortedPlaces.ElementAt(random.Next(sortedPlaces.Count));
        }

        /// <summary>
        /// Calculating the total reachable score with the current time
        /// </summary>
        /// <param name="currentTimeSpent"></param>
        /// <returns></returns>
        public double totalScore(TimeSpan currentTimeSpent)
        {
            double distance = 0;
            double timebonus = 0;
            if (target.Distance != null)
                distance = (double)target.Distance;
            if (currentTimeSpent.TotalSeconds < maximumTime.TotalSeconds)
                timebonus = maximumTime.TotalSeconds - currentTimeSpent.TotalSeconds;

            return distance * 10 + timebonus * 15;
        }
    }

    class MapAssigment : Assigment
    {
        public MapAssigment()
        {
            maxDistance = 5000;
            minDistance = 1000;
            maxSpeed = 40;
        }

        public override void fillDescription()
        {
            description =
                "Walk to the marked point on the map, " +
                $"\n Bonus if reached within {maximumTime.TotalMinutes} minutes." +
                $"\n total score could be {totalScore(new TimeSpan())}!";
        }
    }


    class SearchAssigment : Assigment
    {
        public SearchAssigment()
        {
            maximumTime = new TimeSpan(0,1,0,0);
            maxSpeed = 1000;
            maxDistance = 1000;
            minDistance = 100;
        }


        public override void fillDescription()
        {
            description =
                "Search the given point!" +
                $"\n Name: {target.Name}" +
                $"\n Estimated distance to the target {target.Distance}!" +
                $"\n Bonus if reached within 60 minutes!" +
                $"\n Maximum score is {totalScore(new TimeSpan())}";
        }
    }
}

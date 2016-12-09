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
        public static async Task<Place> pickTargetPlace(Place[] places, GCoordinate currentPosition, int minDistance, int maxDistance)
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
        public static  double totalScore(TimeSpan currentTimeSpent, Place target, TimeSpan maximumTime)
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
    interface IAssigment
    {
        int maxDistance { get; set; }
        int minDistance { get; set; }
        int? maxSpeed { get; set; }
        string description { get; set; }
        Place target { get; set; }
        TimeSpan maximumTime { get; set; }
        
        void fillTarget(Place[] places, GCoordinate currentPosition);

        // TODO ADD VIEW FOR SHOWING ASSIGMEMT INFORMATION
    }

    class MapAssigment : IAssigment
    {
        public int maxDistance { get; set; } = 5000;
        public int minDistance { get; set; } = 1000;
        public int? maxSpeed { get; set; } = 30;
        public string description { get; set; }
        public Place target { get; set; }
        public TimeSpan maximumTime { get; set; }


        


        public async void fillTarget(Place[] places, GCoordinate currentPosition)
        {
            target = await AssigmentHelper.pickTargetPlace(places, currentPosition, minDistance, maxDistance);
            maximumTime = (await GPSHelper.calculateRouteBetween(currentPosition, target.Location)).EstimatedDuration;
            description =
                "Walk to the marked point on the map, " +
                $"\n Bonus if reached within {maximumTime.TotalMinutes} minutes." +
                $"\n total score could be {AssigmentHelper.totalScore(new TimeSpan(), target, maximumTime)}!";
        }


       
    }


    class SearchAssigment : IAssigment
    {
        public int maxDistance { get; set; } = 1000;
        public int minDistance { get; set; } = 100;
        public int? maxSpeed { get; set; } = 100;
        public string description { get; set; }
        public Place target { get; set; }
        public TimeSpan maximumTime { get; set; } = new TimeSpan(0,1,0,0);
    

        public async void fillTarget(Place[] places, GCoordinate currentPosition)
        {
            target = await AssigmentHelper.pickTargetPlace(places, currentPosition, minDistance, maxDistance);
            maximumTime = (await GPSHelper.calculateRouteBetween(currentPosition, target.Location)).EstimatedDuration;
            description =
                "Search the given point!" +
                $"\n Name: {target.Name}" +
                $"\n Estimated distance to the target {target.Distance}!" +
                $"\n Bonus if reached within 60 minutes!" +
                $"\n Maximum score is {AssigmentHelper.totalScore(new TimeSpan(), target, maximumTime)}";
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Maps;
using UWPEindopdracht.GPSConnections;

namespace UWPEindopdracht.Assignments
{
    class AssistedAssignment : Assignment
    {
        private double _nextDistance = 0.0;
        public AssistedAssignment()
        {
            MaxSpeed = null;
            MaxDistance = 1000;
            PictureNeeded = true;
            MinDistance = 200;
            NeededDistance = 50;
            ShowPinPoint = false;
            TimeMultiplier = 1.6;
            Name = "Unknown place";
        }

        

        public override string GetDescription()
        {
            return
                "Search the point given below with instructions!\n" +
                $"\nEstimated distance to the target: {Targets[0].Distance} meter !" +
                $"\nYou'll get a bonus if the point is reached within {MaximumTime.TotalMinutes} minutes!" +
                $"\nYour total score can be {TotalScore(new TimeSpan())}!";
        }
    
        public override async Task<string> GetDistanceText(GCoordinate currentPoint)
        {
            if (Targets == null) return "";
            MapRoute route = await GPSHelper.calculateRouteBetween(currentPoint, Targets[0].Location);
            if (route == null) return "Unreachable!";
            if (route.LengthInMeters >= _nextDistance)
            {
                _nextDistance = route.LengthInMeters;
                return "HOT";
            }
            if (route.LengthInMeters < _nextDistance)
            {
                _nextDistance = route.LengthInMeters;
                return "COLD";
            }
            return "Unreachable!";
        }
    }
}

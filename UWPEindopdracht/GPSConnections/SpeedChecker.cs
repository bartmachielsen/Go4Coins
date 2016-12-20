using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPEindopdracht.GPSConnections
{
    class SpeedChecker
    {
        private List<double> _speeds = new List<double>();
        private List<DateTime> _times = new List<DateTime>();
        private int _measurementsNeeded = 5;
        public int MaxSpeed { get; set; } = 60;
        public SpeedException.WarningLevel CurrentWarningLevel = SpeedException.WarningLevel.None;
        private int _lastWarning;
        private int _amountToWait = 20;
        public void RegisterLocationChange(GCoordinate coordinate)
        {
            if (coordinate.speed == null) return;
            var speed = coordinate.speed.Value*3.6;
            if (_speeds.Count != 0 && !(Math.Abs(speed - _speeds.Last()) > 0)) return;
            _speeds.Add(speed);
            System.Diagnostics.Debug.WriteLine("SPEED: " + speed);
            if (_speeds.Count > _measurementsNeeded)
            {
                double average = _speeds.GetRange(_speeds.Count - _measurementsNeeded, _measurementsNeeded).Average();
                System.Diagnostics.Debug.WriteLine("AVERAGE " + average);
                if (average > MaxSpeed)
                {
                    if (CurrentWarningLevel != SpeedException.WarningLevel.Block && (_speeds.Count > _lastWarning+_amountToWait || _lastWarning == 0))
                    {
                        _lastWarning = _speeds.Count;
                        CurrentWarningLevel = (SpeedException.WarningLevel) ((int) CurrentWarningLevel + 1);
                        throw new SpeedException(CurrentWarningLevel);
                    }
                }
            }
        }
    }

    class SpeedException : Exception
    {
        public enum WarningLevel
        {
            None, Warning, Critical, Block
        }

        public WarningLevel Warning;

        public SpeedException(WarningLevel warning) : base("Speed exception, user is going to fast!")
        {
            Warning = warning;
        }
    }
}

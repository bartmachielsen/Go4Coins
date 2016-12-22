using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Newtonsoft.Json;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.Places;

namespace UWPEindopdracht.Multiplayer
{
    public class MultiplayerAssignmentDetails : Assignment, INotifyPropertyChanged
    {
        public List<string> Participants = new List<string>();
        public int MaxJoiners = 5;
        public bool Closed = false;



        [JsonIgnore]
        public string Id;

        [JsonIgnore]
        public string CurrentUser;

        [JsonIgnore] public bool syncNeeded = false;

        [JsonIgnore]
        public bool Available => (Participants.Count) < MaxJoiners && 
                                 Administrator != CurrentUser && CurrentUser != null;

        [JsonIgnore]
        public string ButtonText {get {
            if (Participants.Contains(CurrentUser))
            {
                return "rejoin";
            }
            else
            {
                return "Join";
            } } }

        public string Description { get; private set; } = "Multiplayer game";
    
        public string Administrator;

        public MultiplayerAssignmentDetails(int maxJoiners, string id, string administrator)
        {
            MaxDistance = 3000;
            MinDistance = 800;
            NeededDistance = 30;
            MaxSpeed = 40;
            TimeMultiplier = 0.6;
            MaxJoiners = maxJoiners;
            Id = id;
            Administrator = administrator;
            if(!Participants.Contains(Administrator))
                Participants.Add(Administrator);
        }
        
        public void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public override string GetDescription()
        {
            Name = Targets[0].Name;
            var span = GetSpentTime();
            if (span < TimeSpan.Zero)
                span = TimeSpan.Zero;
            return
                "Walk to the marked point on the map! with other players!\n" +
                $"\nYou'll get a bonus if the point is reached within {MaximumTime.TotalMinutes} minutes!" +
                $"\nYour total score can be {TotalScore(span)}!\n" +
                $"Amount of other users: {Participants.Count-1}";
        }
        
        public event PropertyChangedEventHandler PropertyChanged;


        public void Merge(MultiplayerAssignmentDetails assignment)
        {
            Name = assignment.Name;
            OnPropertyChanged("Name");
            Participants = assignment.Participants;
            OnPropertyChanged("Participants");
            OnPropertyChanged("Joiners");
            MaxJoiners = assignment.MaxJoiners;
            OnPropertyChanged("MaxJoiners");
            Description = assignment.Description;
            OnPropertyChanged("Description");
            Administrator = assignment.Administrator;
            OnPropertyChanged("Administrator");
            OnPropertyChanged("Available");
        }

        public override async Task<Place[]> PickTargetPlace(List<Place> places, GCoordinate currentPosition)
        {
            if (Targets != null)
                return Targets;
            if (Administrator == CurrentUser)
            {
                syncNeeded = true;
                return await base.PickTargetPlace(places, currentPosition);
            }
            return null;
        }
        public override bool LoadPlaces()
        {
            return CurrentUser == null || Administrator == CurrentUser;
        }

        public override void StartAssignment()
        {
            if (CurrentUser == Administrator)
            {
                base.StartAssignment();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("NO ADMIN SO NO STARTING!");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Newtonsoft.Json;

namespace UWPEindopdracht.Multiplayer
{
    public class MultiplayerAssignmentDetails : Assignment, INotifyPropertyChanged
    {
        public List<string> Participants = new List<string>();
        public List<string> Joiners = new List<string>();
        public int MaxJoiners = 5;
        [JsonIgnore]
        public string Id;

        [JsonIgnore]
        public bool Available
        {
            get { return (Joiners.Count + Participants.Count) < MaxJoiners; }
        }

        public string Description { get; set; } = "Multiplayer game";
    
        public string Administrator;

        public MultiplayerAssignmentDetails(int maxJoiners, string id, string administrator)
        {
            MaxJoiners = maxJoiners;
            Id = id;
            Administrator = administrator;
        }
        
        public void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public override string GetDescription()
        {
           return Description;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;


        public void Merge(MultiplayerAssignmentDetails assignment)
        {
            Name = assignment.Name;
            OnPropertyChanged("Name");
            Participants = assignment.Participants;
            OnPropertyChanged("Participants");
            Joiners = assignment.Joiners;
            OnPropertyChanged("Joiners");
            MaxJoiners = assignment.MaxJoiners;
            OnPropertyChanged("MaxJoiners");
            Description = assignment.Description;
            OnPropertyChanged("Description");
            Administrator = assignment.Administrator;
            OnPropertyChanged("Administrator");
            OnPropertyChanged("Available");
        }
    }
}

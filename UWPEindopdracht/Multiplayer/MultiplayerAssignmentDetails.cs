using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UWPEindopdracht.Multiplayer
{
    class MultiplayerAssignmentDetails : Assignment
    {
        public List<string> Participants = new List<string>();
        public int MaxJoiners = 5;
        [JsonIgnore]
        private string _id;
        public string Administrator;

        public MultiplayerAssignmentDetails(int maxJoiners, string id, string administrator)
        {
            MaxJoiners = maxJoiners;
            _id = id;
            Administrator = administrator;
        }

        public override void FillDescription()
        {
            Description = "[DESCRIPTION]";
            // TODO SET DESCRIPTION
        }
    }
}

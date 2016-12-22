using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using UWPEindopdracht.DataConnections;
using UWPEindopdracht.GPSConnections;

namespace UWPEindopdracht.Multiplayer
{
    public class MultiplayerData
    {
        public static int ServerTimeOut = 4;
        public static bool NoInternetConfirmed = false;

        public RestDBConnector Db = new RestDBConnector();
        public PlaceLoader placeLoader = new PlaceLoader();
        public List<User> Users = new List<User>();
        public List<MultiplayerAssignmentDetails> MultiplayerAssignmentDetailses;
        public User User;
        public List<Reward> Rewards;
        public DateTime LastLocationSync = DateTime.Now;


        public async Task RegisterMultiplayerUser()
        {

            var localSettings =
                ApplicationData.Current.LocalSettings;
            if (Db == null)
                return;

            if (!localSettings.Values.ContainsKey("multiplayerID"))
            {
                try
                {
                    User = await Db.UploadUser(User);
                    localSettings.Values["multiplayerID"] = User.id;
                    MultiplayerData.NoInternetConfirmed = false;
                }
                catch (NoInternetException)
                {
                   MapPage.InternetException();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }
            else
            {
                try
                {
                    User = await Db.GetUser((string)localSettings.Values["multiplayerID"]);
                    System.Diagnostics.Debug.WriteLine(User.Name);
                    MultiplayerData.NoInternetConfirmed = false;

                }
                catch (NoResponseException)
                {
                    User = null;
                }
                catch (NoInternetException)
                {
                    MapPage.InternetException();
                    User = null;
                }
                catch (Exception)
                {
                    User = null;
                }
                if (User == null)
                {
                    User = await Db.UploadUser(null);
                    localSettings.Values["multiplayerID"] = User.id;
                }
            }

        }

        public async Task UpdateMultiplayerServer(GCoordinate coordinate)
        {
            if (User == null) return;
            LastLocationSync = DateTime.Now;
            User.Location = coordinate;
            try
            {
                await Db.UpdateUser(User);
                MultiplayerData.NoInternetConfirmed = false;
            }
            catch (NoResponseException)
            {
            }
            catch (NoInternetException)
            {
                MapPage.InternetException();
            }
            catch (Exception)
            {
            }
        }

        public async Task LoadRewards()
        {
            try
            {
                Rewards = await Db.GetRewards();
                NoInternetConfirmed = false;
            }
            catch (NoInternetException)
            {
                MapPage.InternetException();
            }
        }

        public async Task LoadAssignments()
        {
            MultiplayerAssignmentDetailses = await Db.GetMultiplayerAssignments();
        }

    }
}

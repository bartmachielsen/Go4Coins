using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using UWPEindopdracht.DataConnections;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.JSON;
using UWPEindopdracht.Multiplayer;

namespace UWPEindopdracht
{
    public sealed partial class MapPage : Page
    {
        private static bool _follow = false;
        private static int _serverTimeOut = 3;


        private readonly Assignment _assignment;
        private User _user = new User(null, "TestUser", new GCoordinate(0, 0));
        private readonly RestDBConnector _db = new RestDBConnector();
        private MapIcon _userLocation;
        private List<User> _users = new List<User>();
        private List<Reward> _rewards;
        private bool _noInternetConfirmed = false;
        
        private DateTime _lastLocationSync = DateTime.Now;
        private readonly DispatcherTimer _onTargetNotificationTimer = new DispatcherTimer();

        public MapPage()
        {
            
            LoadMultiplayerDetails();

            InitializeComponent();
            _assignment = new MapAssignment();
            var locator = new Geolocator {DesiredAccuracyInMeters = 10};

            locator.PositionChanged += Locator_PositionChanged;

            SetLocation();
            MapControl.ZoomInteractionMode = MapInteractionMode.GestureAndControl;
            MapControl.ZoomLevel = 13;
        }

        private string DistanceText { get; set; } = "0 km";
        private string TimeText { get; set; } = "00:00";

        private async Task LoadRewards()
        {
            try
            {
                _rewards = await _db.GetRewards();
                _noInternetConfirmed = false;
            }
            catch (NoInternetException)
            {
                InternetException();
            }
        }

        private async void LoadMultiplayerDetails()
        {
           
            var localSettings =
                ApplicationData.Current.LocalSettings;
            if (_user == null || _db == null)
            {
                return;
            }
            if (!localSettings.Values.ContainsKey("multiplayerID"))
            {
                try
                {
                    _user = await _db.UploadUser(_user);
                    localSettings.Values["multiplayerID"] = _user.id;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            else
            {
                _user.id = (string) localSettings.Values["multiplayerID"];
                try
                {
                    await _db.UpdateUser(_user);
                    _noInternetConfirmed = false;
                }
                catch (NoResponseException)
                {
                    _user.id = null;
                    _user = await _db.UploadUser(_user);
                    localSettings.Values["multiplayerID"] = _user.id;
                }
                catch (NoInternetException)
                {
                    InternetException();
                }
            }
            _user.Self = true;
            _users.Add(_user);

            await LoadRewards();

            LiveUpdateUser();
        }

        private async void LiveUpdateUser()
        {
            
            while (true)
            {
                await Task.Delay(_serverTimeOut*1000);
                await UpdateUserDetails();
            }
        }

        private async void InternetException()
        {
            if (!_noInternetConfirmed)
            {
                await new MessageDialog("No internet connection found", "Internet connection error").ShowAsync();
                _noInternetConfirmed = true;
            }
        }
        private async Task UpdateUserDetails()
        {
            try
            {
                _users = await _db.GetUsers(_users);
                _noInternetConfirmed = false;
            }
            catch (NoInternetException)
            {
                InternetException();
            }
            catch (NoResponseException)
            {
                Debug.WriteLine("Got no response from database! but continue because shit happens");
            }

            if (_users == null)
            {
                return;
            }
            foreach (var user in _users)
                if (user.id != _user.id)
                {
                    var geopoint = GPSHelper.getPointOutLocation(user.location);
                    if ((DateTime.Now - user.lastSynced) >= TimeSpan.FromSeconds(_serverTimeOut*5))
                    {
                        if (user.Icon != null)
                        {
                            MapControl.MapElements.Remove(user.Icon);
                            user.Icon = null;
                        }

                    }
                    else
                    {
                        Debug.WriteLine($"{user.id} is loaded! {DateTime.Now-user.lastSynced}");
                        if (user.Icon == null)
                        {
                            user.Icon = new MapIcon
                            {
                                Location = geopoint,
                                Title = user.Name
                            };
                            MapControl.MapElements.Add(user.Icon);
                        }
                        else
                        {
                            user.Icon.Location = geopoint;
                        }
                    }
                }
        }

        private async void UpdateMultiplayerServer(GCoordinate coordinate)
        {
            _lastLocationSync = DateTime.Now;
            _user.location = coordinate;
            try
            {
                await _db.UpdateUser(_user);
                _noInternetConfirmed = false;
            }
            catch (NoResponseException)
            {
                var localSettings =
                ApplicationData.Current.LocalSettings;
                _user.id = null;
                _user = await _db.UploadUser(_user);
                localSettings.Values["multiplayerID"] = _user.id;
            }
            catch (NoInternetException)
            {
                InternetException();
            }

        }

        private void PlacePinPoints(Geopoint location)
        {
            if (_userLocation == null)
            {
                _userLocation = new MapIcon {Title = "Your Location"};
                MapControl.MapElements.Add(_userLocation);
            }
            if ((_assignment?.Target != null) && _assignment.ShowPinPoint)
            {
                foreach (var target in _assignment.Target)
                {
                    if (target.Icon == null)
                    {
                        target.Icon = new MapIcon()
                        {
                            Title = target.Name,
                            Location = GPSHelper.getPointOutLocation(target.Location)
                        };
                        if (!string.IsNullOrEmpty(target.IconLink))
                            target.Icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri(target.IconLink));
                        MapControl.MapElements.Add(target.Icon);
                    }
                }
            }
            

            _userLocation.Location = location;
        }

        private async void SetLocation()
        {
            var loc = await GPSHelper.getLocationOriginal();
            if (loc != null)
            {
                MapControl.Center = loc.Coordinate.Point;
                try
                {
                    var places = await PlaceLoader.GetPlaces(GPSHelper.GetGcoordinate(loc.Coordinate.Point));
                    Debug.WriteLine($"Loaded {places.Count} points!");
                    await _assignment.FillTarget(places, GPSHelper.GetGcoordinate(loc.Coordinate.Point));
                    ChangeDistance(GPSHelper.GetGcoordinate(loc.Coordinate.Point));
                    ChangeTime();

                    //start time change timer
                    var timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(1)
                    };
                    timer.Tick += delegate
                    {
                        if (_assignment != null)
                            ChangeTime();
                        else
                            timer.Stop();
                    };
                    timer.Start();
                }
                catch (ApiLimitReached)
                {
                    await new MessageDialog("Api Limit is reached!", "Api Exception").ShowAsync();
                }
                catch (InvalidApiKeyException)
                {
                    await new MessageDialog("Api key is invalid!", "Api Exception").ShowAsync();
                }
                PlacePinPoints(MapControl.Center);
            }
            else
            {
                await new MessageDialog("No GPS connection!", "Can't get your location").ShowAsync();
                //SetLocation();
                // TODO EXIT APPLICATION BECAUSE NO GPS SIGNAL OR TRY AGAIN A FEW TIMES
            }
        }

        private async void ChangeDistance(GCoordinate current)
        {
            if ((_assignment != null) && (_assignment.Target != null))
            {
                var information = await _assignment.GetRouteInformation(current);
                DistanceText = information[1];
                DistanceTextBlock.Text = DistanceText;
                
            }
        }

        private async void ChangeTime()
        {
            if ((_assignment != null) && (_assignment.Target != null))
            {
                var information = await _assignment.GetRouteInformation(null, false);
                TimeText = information[0];
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { TimeTextBlock.Text = TimeText; });
            }
        }

        private async void Locator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            if (DateTime.Now - _lastLocationSync > TimeSpan.FromSeconds(_serverTimeOut))
                UpdateMultiplayerServer(GPSHelper.GetGcoordinate(args.Position.Coordinate.Point));
                
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var location = args.Position.Coordinate.Point;
                if (location != null)
                {
                    if (_follow)
                        MapControl.Center = location;
                    PlacePinPoints(location);
                    ChangeDistance(GPSHelper.GetGcoordinate(location));
                }
            });
        }

        private void SetGeofence(Geopoint point)
        {
            GeofenceMonitor.Current.Geofences.Add(new Geofence("Fence1", new Geocircle(point.Position, 10),
                MonitoredGeofenceStates.Entered, false, new TimeSpan(5)));
            GeofenceMonitor.Current.GeofenceStateChanged += GeofenceActivated;
        }

        private void GeofenceActivated(GeofenceMonitor sender, object args)
        {
            throw new NotImplementedException();
        }

        private void OnTargetButton_Click(object sender, RoutedEventArgs e)
        {
            OnTargetText.Opacity = 1.0;
            OnTargetErrorAnimation.Begin();
        }

        private void MultiplayerToggleButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void GoToAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            var a = new AlbumDialog();
            await a.ShowAsync();
        }

        private async void GoToShopButton_Click(object sender, RoutedEventArgs e)
        {
            var s = new ShopDialog();
            await s.ShowAsync();
        }
    }
}
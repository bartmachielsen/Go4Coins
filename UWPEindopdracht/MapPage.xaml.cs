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
using UWPEindopdracht.Places;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPEindopdracht
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page
    {
        private static bool _follow = false;
        private static int _serverTimeOut = 3;


        private Assignment _assignment;

        private User _user;

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
            if (_db == null)
                return;
            
            if (!localSettings.Values.ContainsKey("multiplayerID"))
            {
                try
                {
                    _user = await _db.UploadUser(_user);
                    localSettings.Values["multiplayerID"] = _user.id;
                    _noInternetConfirmed = false;
                }
                catch (NoInternetException)
                {
                    InternetException();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }else
            {
                try
                {
                    _user = await _db.GetUser((string) localSettings.Values["multiplayerID"]);
                    _noInternetConfirmed = false;
                    if (_user == null)
                    {
                        _user = await _db.UploadUser(null);
                        localSettings.Values["multiplayerID"] = _user.id;
                    }
                }
                catch (NoResponseException)
                {
                    
                }
                catch (NoInternetException)
                {
                    InternetException();
                }
            }
            
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
                CheckIfLocationUpdate(null);
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
                System.Diagnostics.Debug.WriteLine("Got no response from database! but continue because shit happens");
            }

            if (_users == null)
            {
                return;
            }
            foreach (var user in _users)
                if (user.id != _user.id)
                {
                    var geopoint = GPSHelper.getPointOutLocation(user.Location);
                    if ((DateTime.Now - user.LastSynced) >= TimeSpan.FromSeconds(_serverTimeOut*5))
                    {
                        if (user.Icon != null)
                        {
                            MapControl.MapElements.Remove(user.Icon);
                            user.Icon = null;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"{user.id} is loaded! {DateTime.Now-user.LastSynced}");
                        if (user.Icon == null)
                        {
                            user.Icon = new MapIcon
                            {
                                Location = geopoint,
                                Title = user.Name
                            };
                            MapControl.MapElements.Add(user.Icon);

                            if (user.LastState == LastState.Online)
                            {
                                new MessageDialog("New user has come online!", "User has entered this world!").ShowAsync();
                            }
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
            if (_user == null) return;
            _lastLocationSync = DateTime.Now;
            _user.Location = coordinate;
            try
            {
                await _db.UpdateUser(_user);
                _noInternetConfirmed = false;
            }
            catch (NoResponseException)
            {
                //var localSettings =
                //ApplicationData.Current.LocalSettings;
                //_user.id = null;
                //_user = await _db.UploadUser(_user);
                //localSettings.Values["multiplayerID"] = _user.id;
            }
            catch (NoInternetException)
            {
                InternetException();
            }
            catch (Exception)
            {
                
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
                        LoadingAnimation.Visibility = Visibility.Collapsed;
                        LoadingText.Visibility = Visibility.Collapsed;
                        MultiplayerToggleButton.IsEnabled = true;
                        GoToAlbumButton.IsEnabled = true;
                        GoToShopButton.IsEnabled = true;
                        OnTargetButton.IsEnabled = true;
                        MapControl.PanInteractionMode = MapPanInteractionMode.Auto;
                        MapControl.ZoomInteractionMode = MapInteractionMode.Auto;
                    }
                }
            }
            

            _userLocation.Location = location;
        }

        private async void SetAssignment(Geoposition loc)
        {
            try
            {
                var places = await PlaceLoader.GetPlaces(GPSHelper.GetGcoordinate(loc.Coordinate.Point));
                await new GoogleStreetviewConnector().GetURLToSavePicture(places[0]);
                Debug.WriteLine($"Loaded {places.Count} points!");
                try
                {
                    await _assignment.FillTarget(places, GPSHelper.GetGcoordinate(loc.Coordinate.Point));
                }
                catch (NoTargetAvailable)
                {
                    await new MessageDialog("Move to another area! (move +5KM)", "Not enough targets!").ShowAsync();
                    _assignment = null;
                    return;
                }

                await new AssignmentDialog(_assignment,await ImageLoader.GetBestUrlFromPlace(_assignment)).ShowAsync();
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
            /// remove pinpoints!
            PlacePinPoints(loc.Coordinate.Point);
        }

        private async void SetLocation()
        {
            var loc = await GPSHelper.getLocationOriginal();
            if (loc != null)
            {
                MapControl.Center = loc.Coordinate.Point;
                SetAssignment(loc);
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

        private async void CheckIfLocationUpdate(Geopoint point)
        {
            if (DateTime.Now - _lastLocationSync > TimeSpan.FromSeconds(_serverTimeOut))
            {
                if (point == null)
                    point = (await GPSHelper.getLocationOriginal()).Coordinate.Point;
                UpdateMultiplayerServer(GPSHelper.GetGcoordinate(point));
            }

        }

        private async void Locator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            CheckIfLocationUpdate(args.Position.Coordinate.Point);
                

                
            
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

        private void MapControl_OnZoomLevelChanged(MapControl sender, object args)
        {
            MapControl.ZoomLevel = 50;
        }
    }
}
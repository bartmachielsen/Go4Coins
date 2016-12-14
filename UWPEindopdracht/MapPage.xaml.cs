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
using UWPEindopdracht.Multiplayer;

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


        private readonly Assignment _assignment;
        private User _user = new User(null, "TestUser", new GCoordinate(0, 0));
        private readonly RestDBConnector _db = new RestDBConnector();
        private MapIcon _userLocation;
        private List<User> _users = new List<User>();
        private bool _noInternetConfirmed = false;

        private DateTime _lastLocationSync = DateTime.Now;
        private DispatcherTimer _onTargetNotificationTimer = new DispatcherTimer();

        public MapPage()
        {
            LoadMultiplayerDetails();
            InitializeComponent();
            _assignment = new MapAssignment();
            var locator = new Geolocator {DesiredAccuracyInMeters = 10};

            locator.PositionChanged += Locator_PositionChanged;

            SetLocation();
            mapControl.ZoomInteractionMode = MapInteractionMode.GestureAndControl;
            mapControl.ZoomLevel = 13;
        }

        private string _distanceText { get; set; } = "0 km";
        private string _timeText { get; set; } = "00:00";

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
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }
            else
            {
                _user.id = (string) localSettings.Values["multiplayerID"];
            }
            _user.Self = true;
            _users.Add(_user);
            
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
                            mapControl.MapElements.Remove(user.Icon);
                            user.Icon = null;
                        }

                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"{user.id} is loaded! {DateTime.Now-user.lastSynced}");
                        if (user.Icon == null)
                        {
                            user.Icon = new MapIcon
                            {
                                Location = geopoint,
                                Title = user.Name
                            };
                            mapControl.MapElements.Add(user.Icon);
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
                _db.UpdateUser(_user);
                _noInternetConfirmed = false;
            }
            catch (NoInternetException)
            {
                //System.Diagnostics.Debug.WriteLine(e);
                InternetException();
            }

        }

        private void PlacePinPoints(Geopoint location)
        {
            if (_userLocation == null)
            {
                _userLocation = new MapIcon {Title = "Your Location"};
                mapControl.MapElements.Add(_userLocation);
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
                        mapControl.MapElements.Add(target.Icon);
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
                mapControl.Center = loc.Coordinate.Point;
                try
                {
                    var places = await PlaceLoader.GetPlaces(GPSHelper.GetGcoordinate(loc.Coordinate.Point));
                    Debug.WriteLine($"Loaded {places.Count} points!");
                    await _assignment.FillTarget(places, GPSHelper.GetGcoordinate(loc.Coordinate.Point));
                    changeDistance(GPSHelper.GetGcoordinate(loc.Coordinate.Point));
                    changeTime();

                    //start time change timer
                    var timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(1)
                    };
                    timer.Tick += delegate
                    {
                        if (_assignment != null)
                            changeTime();
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
                PlacePinPoints(mapControl.Center);
            }
            else
            {
                await new MessageDialog("No GPS connection!", "Can't get your location").ShowAsync();
                //SetLocation();
                // TODO EXIT APPLICATION BECAUSE NO GPS SIGNAL OR TRY AGAIN A FEW TIMES
            }
        }

        private async void changeDistance(GCoordinate current)
        {
            if ((_assignment != null) && (_assignment.Target != null))
            {
                var information = await _assignment.GetRouteInformation(current);
                _distanceText = information[1];
                DistanceTextBlock.Text = _distanceText;
                
            }
        }

        private async void changeTime()
        {
            if ((_assignment != null) && (_assignment.Target != null))
            {
                var information = await _assignment.GetRouteInformation(null, false);
                _timeText = information[0];
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { TimeTextBlock.Text = _timeText; });
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
                        mapControl.Center = location;
                    PlacePinPoints(location);
                    changeDistance(GPSHelper.GetGcoordinate(location));
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
            if (!_onTargetNotificationTimer.IsEnabled)
            {
                _onTargetNotificationTimer.Start();
                OnTargetText.Visibility = Visibility.Visible;
            }
        }

        private void MultiplayerToggleButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GoToAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
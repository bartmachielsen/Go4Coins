using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Storage;
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
        private readonly Assignment _assignment;
        private MapIcon _targetLocation;
        private User _user = new User(null, "TestUser", new GCoordinate(0, 0));
        private readonly RestDBConnector _db = new RestDBConnector();
        private MapIcon _userLocation;
        private List<User> _users = new List<User>();
        private bool _follow = false;
        private int _serverTimeOut = 3;
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

            if (!localSettings.Values.ContainsKey("multiplayerID"))
            {
                _user = await _db.UploadUser(_user);
                localSettings.Values["multiplayerID"] = _user.id;
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

        private async Task UpdateUserDetails()
        {
            _users = await _db.GetUsers(_users);
            if (_users == null)
            {
                System.Diagnostics.Debug.WriteLine("_USERS IS NULL");
                return;
            }
            foreach (var user in _users)
                if (user.id != _user.id)
                {
                    var geopoint = GPSHelper.getPointOutLocation(user.location);
                    //System.Diagnostics.Debug.WriteLine(DateTime.Now-user.lastSynced);
                    if ((DateTime.Now - user.lastSynced) >= TimeSpan.FromSeconds(_serverTimeOut*4))
                    {
                        System.Diagnostics.Debug.WriteLine(DateTime.Now - user.lastSynced);
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
            Debug.WriteLine("UPDATING USER COORDINATS IN MULTIPLAYER SERVER");
            _db.UpdateUser(_user);
            
        }

        private void PlacePinPoints(Geopoint location)
        {
            if (_userLocation == null)
            {
                _userLocation = new MapIcon {Title = "you"};
                mapControl.MapElements.Add(_userLocation);
            }
            if ((_targetLocation == null) && (_assignment?.Target != null) && _assignment.ShowPinPoint)
            {
                _targetLocation = new MapIcon
                {
                    Title = _assignment.Target.Name,
                    Location = GPSHelper.getPointOutLocation(_assignment.Target.Location)
                };
                mapControl.MapElements.Add(_targetLocation);
            }
            else if ((_assignment?.Target != null) && _assignment.ShowPinPoint && (_targetLocation != null))
            {
                _targetLocation.Location = GPSHelper.getPointOutLocation(_assignment.Target.Location);
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
                    var Places = await PlaceLoader.GetPlaces(GPSHelper.GetGcoordinate(loc.Coordinate.Point));
                    Debug.WriteLine($"Loaded {Places.Count} points!");
                    await _assignment.FillTarget(Places, GPSHelper.GetGcoordinate(loc.Coordinate.Point));
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
        }

        private async void changeDistance(GCoordinate current)
        {
            if ((_assignment != null) && (_assignment.Target != null))
            {
                var information = await _assignment.GetRouteInformation(current);
                _distanceText = information[1];
                DistanceTextBlock.Text = _distanceText;
                Debug.WriteLine($"Changed information {_distanceText}");
            }
        }

        private async void changeTime()
        {
            if ((_assignment != null) && (_assignment.Target != null))
            {
                var information = await _assignment.GetRouteInformation(null, false);
                _timeText = information[0];
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { TimeTextBlock.Text = _timeText; });
                Debug.WriteLine($"Changed information {_timeText}");
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
    }
}
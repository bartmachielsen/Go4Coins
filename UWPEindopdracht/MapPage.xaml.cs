using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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
        private static readonly bool _follow = false;
        private static readonly int _serverTimeOut = 4;

        private bool _dialogClaimed = false;
        private readonly PlaceLoader _placeLoader = new PlaceLoader();
        private Assignment _assignment;

        private User _user;

        private readonly RestDBConnector _db = new RestDBConnector();
        private MapIcon _userLocation;

        private List<User> _users = new List<User>();

        private List<Reward> _rewards;

        private bool _noInternetConfirmed = false;
        
        private DateTime _lastLocationSync = DateTime.Now;

        public MapPage()
        {
            GPSHelper.ClearGeofences();
            LoadMultiplayerDetails();
            
            InitializeComponent();
            
            MapControl.MapElementClick += MapControl_MapElementClick;
            var locator = new Geolocator {DesiredAccuracyInMeters = 10};

            locator.PositionChanged += Locator_PositionChanged;


            SetLocation();
            MapControl.ZoomInteractionMode = MapInteractionMode.GestureAndControl;
            MapControl.ZoomLevel = 13;
            LoadAssignments();
        }

        
        private string DistanceText { get; set; } = "0 km";
        private string TimeText { get; set; } = "00:00";

        private async Task LoadAssignments()
        {
            //_db.UploadMultiplayerAssignmentDetail(new MultiplayerAssignmentDetails(5, "test", "admin"));
            var list = await _db.GetMultiplayerAssignments();
            System.Diagnostics.Debug.WriteLine(list.Count);
        }


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
                    System.Diagnostics.Debug.WriteLine(_user.Name);
                    _noInternetConfirmed = false;
                    
                }
                catch (NoResponseException)
                {
                    _user = null;
                }
                catch (NoInternetException)
                {
                    InternetException();
                    _user = null;
                }
                catch (Exception)
                {
                    _user = null;
                }
                if (_user == null)
                {
                    _user = await _db.UploadUser(null);
                    localSettings.Values["multiplayerID"] = _user.id;
                }
            }
            

            await LoadRewards();

            LiveUpdateUser();
        }

        private async void LiveUpdateUser()
        {
            
            while (true)
            {
                await Task.Delay(_serverTimeOut*1000);
                await UpdateUserDetails();
                await CheckIfLocationUpdate(null);
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
                return;
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
                    if ((DateTime.Now - user.LastSynced) >= TimeSpan.FromSeconds(_serverTimeOut*3))
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
                                // TODO SHOW NEW USER NOTIFICATION
                                //ShowUserDetails(user, true);
                            }
                        }
                        else
                        {
                            user.Icon.Title = user.Name;
                            user.Icon.Location = geopoint;
                        }
                    }
                }
        }

        private void ShowUserDetails(User user, bool newUser = true, bool self = false)
        {
            ContentDialog dialog = new UserDialog(user,newUser);

            if(self)
                dialog = new MultiplayerSettings(user);
            ShowDialog(dialog);
        }

        private async void ShowDialog(ContentDialog dialog)
        {
            while (_dialogClaimed) { }
            if (!_dialogClaimed)
            {
                _dialogClaimed = true;
                await dialog.ShowAsync();
                _dialogClaimed = false;
            }
        }
       
        private void MapControl_MapElementClick(MapControl sender, MapElementClickEventArgs args)
        {
            foreach (var user in _users)
            {
                if (user.Icon == null) continue;
                if (args.MapElements.All(element => element != user.Icon)) continue;
                ShowUserDetails(user, false);
                return;
            }
            if(_assignment != null)
                foreach (var target in _assignment.Target)
                    foreach (var element in args.MapElements)
                        if (target.Icon != null && target.Icon == element)
                            ShowDialog(new PlaceDialog(target));
                    
                
            if(args.MapElements.All(element => element == _user.Icon))
                ShowUserDetails(_user,false,true);
        }

        private async Task UpdateMultiplayerServer(GCoordinate coordinate)
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

        private void RemovePinPoints(Assignment oldAssignment)
        {
            foreach (var place in oldAssignment.Target)
            {
                MapControl.MapElements.Remove(place.Icon);
                place.Icon = null;
            }
        }

        private void PlacePinPoints(Geopoint location)
        {
            if (_userLocation == null)
            {
                _userLocation = new MapIcon {Title = "Your Location"};
                MapControl.MapElements.Add(_userLocation);
            }
            if (_user != null && _user.Icon == null && _userLocation != null)
                _user.Icon = _userLocation;
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
                        MapControl.RotateInteractionMode = MapInteractionMode.Auto;
                        MapControl.TiltInteractionMode = MapInteractionMode.Auto;
                    }
                }
            }
            _userLocation.Location = location;
        }

        private async Task SetAssignment(Geoposition loc, Assignment newAssignment)
        {
            if(_assignment != null)
                RemovePinPoints(_assignment);
            await _placeLoader.LoadPlaces(GPSHelper.GetGcoordinate(loc.Coordinate.Point));
            
            try
            {
                await newAssignment.FillTarget(_placeLoader.Places, GPSHelper.GetGcoordinate(loc.Coordinate.Point));
            }
            catch (NoTargetAvailable)
            {
                await new MessageDialog("Move to another area! (move +5KM)", "Not enough targets!").ShowAsync();
                newAssignment = null;
                return;
            }
            var dialog = new AssignmentDialog(newAssignment, await ImageLoader.GetBestUrlFromPlace(newAssignment));
            while (_dialogClaimed) { }
            if (!_dialogClaimed)
            {
                _dialogClaimed = true;
                await dialog.ShowAsync();
                _dialogClaimed = false;
            }
            if (!dialog.accepted)
            {
                await SetAssignment(loc, newAssignment);
                return;
            }
            else
            {
                newAssignment.StartAssignment();
                _assignment = newAssignment;
                PlacePinPoints(loc.Coordinate.Point);

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
        }

        private async void SetLocation()
        {
            var loc = await GPSHelper.getLocationOriginal();
            if (loc != null)
            {
                MapControl.Center = loc.Coordinate.Point;
                await SetAssignment(loc,new MapAssignment());
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

        private async Task CheckIfLocationUpdate(Geopoint point)
        {
            if (DateTime.Now - _lastLocationSync <= TimeSpan.FromSeconds(_serverTimeOut)) return;
            if (point == null)
                point = (await GPSHelper.getLocationOriginal()).Coordinate.Point;
            await UpdateMultiplayerServer(GPSHelper.GetGcoordinate(point));
        }

        private async void Locator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            //CheckIfLocationUpdate(args.Position.Coordinate.Point);
              // TODO NEEDED ?  
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
            if (_assignment == null) return;
            if (_assignment.CurrentLocation == null)
            {
                OnTargetText.Text = "Not close enough!";
                OnTargetText.Foreground = new SolidColorBrush(Colors.Red);
                OnTargetText.Opacity = 1.0;
                OnTargetErrorAnimation.Begin();
            }
            else if (!_assignment.RegisterTarget(_assignment.CurrentLocation))
            {
                OnTargetText.Text = "already reached!";
                OnTargetText.Foreground = new SolidColorBrush(Colors.Red);
                OnTargetText.Opacity = 1.0;
                OnTargetErrorAnimation.Begin();
            }
            else
            {
                OnTargetText.Text = "reached!";
                OnTargetText.Foreground = new SolidColorBrush(Colors.Green);
                OnTargetText.Opacity = 1.0;
                OnTargetErrorAnimation.Begin();

                if (_assignment.AssignmentFinished())
                {
                    new AssignmentDialog(_assignment, null).ShowAsync();
                    //TODO ADD SCORE TO USER!
                }
            }
        }

        private void MultiplayerToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // show input dialog for inputting name
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
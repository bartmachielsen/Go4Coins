using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using UWPEindopdracht.DataConnections;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.Multiplayer;
using UWPEindopdracht.Places;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPEindopdracht
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page
    {
        public bool Follow = false;
        private string _distanceText { get; set; } = "0 km";
        private string _timeText { get; set; } = "00:00";
        private User _user = new User(null, "TestUser", new GCoordinate(0, 0));
        private Assignment _assignment;
        private List<User> _users = new List<User>();

        private MapIcon _userLocation;
        private MapIcon _targetLocation;

        public MapPage()
        {
            LoadMultiplayerDetails();
            this.InitializeComponent();
            _assignment = new MapAssignment();
            var locator = new Geolocator() { DesiredAccuracyInMeters = 10, ReportInterval = 100};
            
            locator.PositionChanged += Locator_PositionChanged;
            
            SetLocation();
            mapControl.ZoomInteractionMode = MapInteractionMode.GestureAndControl;
            mapControl.ZoomLevel = 13;


            
        }

        private async void LoadMultiplayerDetails()
        {
            var db = new RestDBConnector();
            Windows.Storage.ApplicationDataContainer localSettings =
                Windows.Storage.ApplicationData.Current.LocalSettings;

            if (!localSettings.Values.ContainsKey("multiplayerID"))
            {
                _user = await db.UploadUser(_user);
                localSettings.Values["multiplayerID"] = _user.id;
            }
            else
            {
                _user.id = (string)localSettings.Values["multiplayerID"];
            }
            List<User> users = await db.GetUsers();
            foreach (var user in users)
            {
                if (user.id != _user.id)
                {
                    var geopoint = GPSHelper.getPointOutLocation(user.location);
                    MapIcon icon = new MapIcon()
                    {
                        Location = geopoint,
                        Title = user.Name
                    };
                    mapControl.MapElements.Add(icon);
                }
            }
        }

        private async void UpdateMultiplayerServer(GCoordinate coordinate)
        {
            var db = new RestDBConnector();
            _user.location = coordinate;
            System.Diagnostics.Debug.WriteLine("UPDATING USER COORDINATS IN MULTIPLAYER SERVER");
            db.UpdateUser(_user);
        }

        private void PlacePinPoints(Geopoint location)
        {
            if (_userLocation == null)
            {
                _userLocation = new MapIcon() {Title = "you"};
                mapControl.MapElements.Add(_userLocation);
            }
            if (_targetLocation == null && _assignment?.Target != null && _assignment.ShowPinPoint)
            {
                _targetLocation = new MapIcon()
                {
                    Title = _assignment.Target.Name,
                    Location =  GPSHelper.getPointOutLocation(_assignment.Target.Location)
                };
                mapControl.MapElements.Add(_targetLocation);
            }else if (_assignment?.Target != null && _assignment.ShowPinPoint && _targetLocation != null)
            {
                _targetLocation.Location = GPSHelper.getPointOutLocation(_assignment.Target.Location);
            }
            
            _userLocation.Location = location;
            
        }

        private async void SetLocation()
        {
            var loc = (await GPSHelper.getLocationOriginal());
            if (loc != null)
            {
                mapControl.Center = loc.Coordinate.Point;
                try
                {
                    var Places = await PlaceLoader.GetPlaces(GPSHelper.GetGcoordinate(loc.Coordinate.Point));
                    System.Diagnostics.Debug.WriteLine($"Loaded {Places.Count} points!");
                    await _assignment.FillTarget(Places, GPSHelper.GetGcoordinate(loc.Coordinate.Point));
                    changeDistance(GPSHelper.GetGcoordinate(loc.Coordinate.Point));
                    changeTime();

                    //start time change timer
                    var timer = new DispatcherTimer()
                    {
                        Interval = TimeSpan.FromSeconds(1)
                    };
                    timer.Tick += delegate(object sender, object o) 
                    {
                        if (_assignment != null)
                        {
                            changeTime();
                        }
                        else
                        {
                            timer.Stop();
                        }
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
            if (_assignment != null && _assignment.Target != null)
            {
                string[] information = await _assignment.GetRouteInformation(current);
                _distanceText = information[1];
                DistanceTextBlock.Text = _distanceText;
                System.Diagnostics.Debug.WriteLine($"Changed information {_distanceText}");
            }
        }

        private async void changeTime()
        {
            if (_assignment != null && _assignment.Target != null)
            {
                string[] information = await _assignment.GetRouteInformation(null, false);
                _timeText = information[0];
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    TimeTextBlock.Text = _timeText;
                });
                System.Diagnostics.Debug.WriteLine($"Changed information {_timeText}");
            }
        }

        private async void Locator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            UpdateMultiplayerServer(GPSHelper.GetGcoordinate(args.Position.Coordinate.Point));
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var location = args.Position.Coordinate.Point;
                if (location != null)
                {
                    if (Follow)
                        mapControl.Center = location;
                    PlacePinPoints(location);
                    changeDistance(GPSHelper.GetGcoordinate(location));
                }
                
            });
        }

        private void SetGeofence(Geopoint point)
        {
            GeofenceMonitor.Current.Geofences.Add(new Geofence("Fence1", new Geocircle(point.Position, 10), MonitoredGeofenceStates.Entered, false, new TimeSpan(5)));
            GeofenceMonitor.Current.GeofenceStateChanged += GeofenceActivated;
        }

        private void GeofenceActivated(GeofenceMonitor sender, object args)
        {
            throw new NotImplementedException();
        }
    }

    
}

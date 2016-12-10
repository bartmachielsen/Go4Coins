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
using UWPEindopdracht.Places;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPEindopdracht
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page
    {
        public bool follow = false;
        public List<Place> places = new List<Place>();



        public MapPage()
        {
            setLocation();

            this.InitializeComponent();
            var locator = new Geolocator() { DesiredAccuracyInMeters = 10, ReportInterval = 100};
            
            locator.PositionChanged += Locator_PositionChanged;
            
            mapControl.ZoomInteractionMode = MapInteractionMode.GestureAndControl;
            mapControl.ZoomLevel = 13;

            
            

        }

        private async void placePinPoints(Geopoint location)
        {
            mapControl.MapElements.Clear();
            mapControl.MapElements.Add(new MapIcon()
            {
                Title = "you",
                Location = location
            });
            foreach (var place in places)
            {
                mapControl.MapElements.Add(new MapIcon()
                {
                    Title = place.Name,
                    Location = GPSHelper.getPointOutLocation(place.Location)
                });
                
            }
           
            
        }
        private async void setLocation()
        {
            var loc = (await GPSHelper.getLocationOriginal());
            if (loc != null)
            {
                mapControl.Center = loc.Coordinate.Point;
                try
                {
                    places.AddRange(
                        await
                            new GooglePlacesConnector().GetPlaces(5000,
                                new GCoordinate(loc.Coordinate.Point.Position.Latitude,
                                    loc.Coordinate.Point.Position.Longitude)));
                    System.Diagnostics.Debug.WriteLine($"Loaded {places.Count} points!");
                }
                catch (ApiLimitReached)
                {
                    await new MessageDialog("Api Limit is reached!", "Api Exception").ShowAsync();
                }
                catch (InvalidApiKeyException)
                {
                    await new MessageDialog("Api key is invalid!", "Api Exception").ShowAsync();
                }
                placePinPoints(mapControl.Center);

            }
        }
        private async void Locator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var location = args.Position.Coordinate.Point;
                if (location != null)
                {
                    if (follow)
                        mapControl.Center = location;
                    placePinPoints(location);
                }
                
            });
        }

        private void setGeofence(Geopoint point)
        {
            GeofenceMonitor.Current.Geofences.Add(new Geofence("Fence1", new Geocircle(point.Position, 10), MonitoredGeofenceStates.Entered, false, new TimeSpan(5)));
            GeofenceMonitor.Current.GeofenceStateChanged += geofenceActivated;
        }

        private void geofenceActivated(GeofenceMonitor sender, object args)
        {
            throw new NotImplementedException();
        }
    }

    
}

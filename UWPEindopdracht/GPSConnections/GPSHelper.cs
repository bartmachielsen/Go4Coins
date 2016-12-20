using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Foundation;
using Windows.Services.Maps;
using UWPEindopdracht.Places;

namespace UWPEindopdracht.GPSConnections
{
    /// <summary>
    /// Method for helping with GPS data from the windows phone like getting the location and converting the given coordinate to our coordinate system
    /// </summary>
    class GPSHelper
    {
        /// <summary>
        /// The accuracy we want from the geocoordinator, like we want always to have a measurement with 5 meters accuracy
        /// </summary>
        public static uint desiredAccuracy = 5;

        /// <summary>
        /// Get the location of the user
        /// </summary>
        /// <returns>returns the location of the user in form of <see cref="GCoordinate"/> or <see cref="CivilCoordinate"/></returns>
        public static async Task<GCoordinate> getLocation()
        {
            if (!(await checkGPSState())) return null;
            var locator = new Geolocator() {DesiredAccuracyInMeters = desiredAccuracy};
            return GetGCoordinate(await locator.GetGeopositionAsync());
        }
        /// <summary>
        /// Method for checking if gps receiver is working and the system has granted acces
        /// </summary>
        /// <returns>returns True if is allowed, False if nog working</returns>
        private static async Task<bool> checkGPSState()
        {
            var accesstate = await Geolocator.RequestAccessAsync();
            if (accesstate != GeolocationAccessStatus.Allowed) return false;
            return true;
        }
        

        /// <summary>
        /// Method for converting a GCoordinate to a Geopoint needed for a map
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public static Geopoint getPointOutLocation(GCoordinate coordinates)
        {
            return new Geopoint(new BasicGeoposition() { Latitude = coordinates.lati, Longitude = coordinates.longi});
        }
        /// <summary>
        /// Get the location of the user
        /// </summary>
        /// <returns>returns the location of the user in form of <see cref="GCoordinate"/> or <see cref="Geoposition"/></returns>
        public static async Task<Geoposition> getLocationOriginal()
        {
            if (!(await checkGPSState())) return null;
            var locator = new Geolocator() { DesiredAccuracyInMeters = desiredAccuracy };
            try
            {
                return await locator.GetGeopositionAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Convers a Geoposition from windows to our own GCoordinate or CivilCoordinate
        /// <see cref="CivilCoordinate"/>
        /// <see cref="Geocoordinate"/>
        /// </summary>
        /// <param name="coordinate">the position in form of <see cref="Geoposition"/></param>
        /// <returns>returns the location converted to <see cref="GCoordinate"/> or <see cref="CivilCoordinate"/></returns>
        public static GCoordinate GetGCoordinate(Geoposition coordinate)
        {
            if (coordinate.CivicAddress != null)
            {
                return new CivilCoordinate(
                    coordinate.Coordinate.Point.Position.Latitude,
                    coordinate.Coordinate.Point.Position.Longitude,
                    null,
                    coordinate.CivicAddress.City,
                    coordinate.CivicAddress.Country,
                    coordinate.CivicAddress.PostalCode)
                {
                    speed = coordinate.Coordinate.Speed,
                    altitude = coordinate.Coordinate.Point.Position.Altitude,
                    accuracy = coordinate.Coordinate.Accuracy

                };
            }
            return new GCoordinate(
                    coordinate.Coordinate.Point.Position.Latitude,
                    coordinate.Coordinate.Point.Position.Longitude)
            {
                speed = coordinate.Coordinate.Speed,
                altitude = coordinate.Coordinate.Point.Position.Altitude,
                accuracy = coordinate.Coordinate.Accuracy

            };
        }

        public static async Task<string> GetCityName(GCoordinate coordinate)
        {
            var result = await MapLocationFinder.FindLocationsAtAsync(getPointOutLocation(coordinate));
            return result.Status == MapLocationFinderStatus.Success ? result.Locations.ElementAt(0).Address.Town : null;
        }
        public static async Task<MapRoute> calculateRouteBetween(GCoordinate start, GCoordinate end)
        {
            var result = (await MapRouteFinder.GetWalkingRouteAsync(getPointOutLocation(start),getPointOutLocation(end)));
            if (result.Status == MapRouteFinderStatus.Success)
                return result.Route;
            return null;
        }

        public static GCoordinate GetGcoordinate(Geopoint center)
        {
            return new GCoordinate(center.Position.Latitude, center.Position.Longitude);
        }
        /// <summary>
        /// Method for listening to changes from the gps locator
        /// </summary>
        /// <exception cref="GpsNotAllowed">Exception when system has deactivated GPS or user does not allow GPS to this application</exception>
        /// <param name="method"></param>
        public static async void NotifyOnLocationUpdate(Func<Geoposition, Task> method)
        {
            if (!await checkGPSState())
            return;
            Geolocator locator = new Geolocator() { DesiredAccuracyInMeters = desiredAccuracy};
            locator.PositionChanged +=
                (Geolocator sender, PositionChangedEventArgs args) => { method.Invoke(args.Position); };
        }
        public static void ClearGeofences()
        {
            GeofenceMonitor.Current.Geofences.Clear();
        }

        /// <summary>
        /// Method for listening if the user the route has leaved or exited
        /// </summary>
        /// <param name="route">The route the user is using <seealso cref="Route"/></param> 
        /// <exception cref="GpsNotAllowed">Exception when system has deactivated GPS or user does not allow GPS to this application</exception>
        public static async Task PlaceEntered(Func<Place, Task> notifier, Place place, int distance = 30)
        {
            if (!await checkGPSState())
                return;
            var geofence = new Geofence($"{place.GetHashCode()} notifier", new Geocircle(getPointOutLocation(place.Location).Position, distance), MonitoredGeofenceStates.Entered, true, TimeSpan.FromSeconds(1));

            PlaceGeofence(geofence, notifier, place);
        }

        public static async Task PlaceLeaved(Func<Place, Task> notifier, Place place, int distance = 30)
        {
            if (!await checkGPSState())
                return;
            var geofence = new Geofence($"{place.GetHashCode()} disnotifier", new Geocircle(getPointOutLocation(place.Location).Position, distance), MonitoredGeofenceStates.Exited, true, TimeSpan.FromSeconds(1));

            PlaceGeofence(geofence, notifier, place);
        }

        public static async Task PlaceGeofence(Geofence geofence, Func<Place, Task> notifier, Place place)
        {
            GeofenceMonitor.Current.Geofences.Add(geofence);

            TypedEventHandler<GeofenceMonitor, object> listener = null;
            listener = async (GeofenceMonitor monitor, object obj) =>
            {
                foreach (var report in monitor.ReadReports())
                    if (report.Geofence.Id == geofence.Id)
                    {
                        notifier.Invoke(place);
                        GeofenceMonitor.Current.GeofenceStateChanged -= listener;
                    }
            };
            GeofenceMonitor.Current.GeofenceStateChanged += listener;
        }

       
    }


}




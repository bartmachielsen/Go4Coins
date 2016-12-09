using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

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
            return getGCoordinate(await locator.GetGeopositionAsync());
        }

        private static async Task<bool> checkGPSState()
        {
            var accesstate = await Geolocator.RequestAccessAsync();
            if (accesstate != GeolocationAccessStatus.Allowed) return false;
            return true;
        }

        public static async Task<Geoposition> getLocationOriginal()
        {
            if (!(await checkGPSState())) return null;
            var locator = new Geolocator() { DesiredAccuracyInMeters = desiredAccuracy };
            return await locator.GetGeopositionAsync();
        }

        /// <summary>
        /// Convers a Geoposition from windows to our own GCoordinate or CivilCoordinate
        /// <see cref="CivilCoordinate"/>
        /// <see cref="Geocoordinate"/>
        /// </summary>
        /// <param name="coordinate">the position in form of <see cref="Geoposition"/></param>
        /// <returns>returns the location converted to <see cref="GCoordinate"/> or <see cref="CivilCoordinate"/></returns>
        private static GCoordinate getGCoordinate(Geoposition coordinate)
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
    }


}

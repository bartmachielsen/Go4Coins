using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Security.Cryptography.Core;
using Windows.UI.Xaml.Controls.Maps;

namespace UWPEindopdracht.GPSConnections
{
    /// <summary>
    /// GCoordinate is a base coordinate with only latitude and longitude
    /// </summary>
    public class GCoordinate
    {
        /// <summary>
        /// latitude of the coordinate
        /// </summary>
        public double lati { get; set; }
        /// <summary>
        /// longitude of the coordinate
        /// </summary>
        public double longi { get; set; }

        /// <summary>
        /// The speed of the user
        /// Warning! can be null
        /// <exception cref="NullReferenceException">When no speed or unavailable</exception>
        /// </summary>
        public double? speed { get; set; }

        /// <summary>
        /// The accuracy of the measurement
        /// Waring! can be null
        /// <exception cref="NullReferenceException">When accuracy is unavailable or not needed</exception>
        /// </summary>
        public double? accuracy { get; set; }

        /// <summary>
        /// The height of the coordinate
        /// Warning! can be null
        /// <exception cref="NullReferenceException">When no height available</exception>
        /// </summary>
        public double? altitude { get; set; }

        /// <summary>
        /// Base constructor for a GCoordinate
        /// </summary>
        /// <param name="lati">latitude of coordinate<see cref="lati"/></param>
        /// <param name="longi">longitude of coordinate<see cref="longi"/></param>
        public GCoordinate(double lati, double longi)
        {
            this.lati = lati;
            this.longi = longi;
        }

        public override bool Equals(object obj)
        {
            Geopoint point;
            GCoordinate other = obj as GCoordinate;
            System.Diagnostics.Debug.WriteLine(Math.Abs(other.lati-lati) + " difference in lati");
            System.Diagnostics.Debug.WriteLine(Math.Abs(other.longi - longi) + " difference in longi");
            if (other == null) return false;
            if (Math.Abs(other.lati - lati) < 1 &&
                Math.Abs(other.longi - longi) < 1 &&
                other.accuracy == accuracy &&
                other.speed == speed &&
                other.altitude == altitude) return true;
            return false;
        }
    }

    /// <summary>
    /// Civil coordinate adds other components to give a coordinate more options and details
    /// </summary>
    class CivilCoordinate : GCoordinate
    {
        /// <summary>
        /// Civil address of the location in form of streetname
        /// </summary>
        private string address { get; set; }
        /// <summary>
        /// Civil address of the location in form of place like cityname
        /// </summary>
        private string place { get; set; }
        /// <summary>
        /// Civil address of the location in form of country like Netherlands and Germany
        /// </summary>
        private string country { get; set; }
        /// <summary>
        /// Civil address of the location in form of postal code like 4040EC
        /// </summary>
        private string postal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lati">Latitude of location <see cref="lati"/></param>
        /// <param name="longi">Longitude of location <see cref="longi"/></param>
        /// <param name="address">Address of location <see cref="address"/></param>
        /// <param name="place">Place of location <see cref="place"/></param>
        /// <param name="country">Country of location <see cref="country"/></param>
        /// <param name="postal">Postal of location <see cref="postal"/></param>
        public CivilCoordinate(double lati, double longi, string address, string place, string country, string postal = null) : base(lati, longi)
        {
            this.address = address;
            this.place = place;
            this.country = country;
            this.postal = postal;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using UWPEindopdracht.GPSConnections;

namespace UWPEindopdracht.Places
{
    public class Place
    {
        private Geopoint _geoPoint;

        /// <summary>
        /// Gcoordinate of the location of the place
        /// <see cref="GCoordinate"/>
        /// </summary>
        public GCoordinate Location { get; set; }
        /// <summary>
        /// Name of the place
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Distance to the place (from the google places api)
        /// Warning! is optional and can be NULL
        /// <exception cref="NullReferenceException"> Exception when distance is not set</exception>
        /// </summary>
        public double? Distance { get; set; }
        /// <summary>
        /// The types that the Place has like supermarket, point of interest, point of worship
        /// </summary>
        private string[] Types { get; set; }
        /// <summary>
        /// The image URL of the place location (Google places URL or other URL)
        /// Warning! is optional and can be NULL
        /// <exception cref="NullReferenceException"> Exception when image is not set</exception>
        /// </summary>
        private string ImageLocation { get; set; }
        /// <summary>
        /// The URL to photos belonging to the place
        /// </summary>
        private string Photo { get; set; }
        /// <summary>
        /// The URL to the icon used for the place
        /// </summary>
        private string IconLink { get; set; }

        /// <summary>
        /// Base Constructor for making a Place
        /// </summary>
        /// <param name="location"><see cref="location"/></param>
        /// <param name="name"><see cref="name"/></param>
        /// <param name="distance"><see cref="distance"/></param>
        /// <param name="types"><see cref="types"/></param>
        /// <param name="imageLocation"><see cref="imageLocation"/></param>
        public Place(GCoordinate location, string name, int? distance, string[] types, string imageLocation, string icon, string photo )
        {
            Location = location;
            Name = name;
            Distance = distance;
            Types = types;
            ImageLocation = imageLocation;
            IconLink = icon;
            Photo = photo;
        }


        public Boolean isCity()
        {
            List<string> types = new List<string>(Types);
            return types.Contains("locality") && types.Contains("political");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPEindopdracht.GPSConnections;

namespace UWPEindopdracht.Places
{
    class Place
    {
        /// <summary>
        /// Gcoordinate of the location of the place
        /// <see cref="GCoordinate"/>
        /// </summary>
        private GCoordinate location { get; set; }
        /// <summary>
        /// Name of the place
        /// </summary>
        private string name { get; set; }
        /// <summary>
        /// Distance to the place (from the google places api)
        /// Warning! is optional and can be NULL
        /// <exception cref="NullReferenceException"> Exception when distance is not set</exception>
        /// </summary>
        private int? distance { get; set; }
        /// <summary>
        /// The types that the Place has like supermarket, point of interest, point of worship
        /// </summary>
        private string[] types { get; set; }
        /// <summary>
        /// The image URL of the place location (Google places URL or other URL)
        /// Warning! is optional and can be NULL
        /// <exception cref="NullReferenceException"> Exception when image is not set</exception>
        /// </summary>
        private string imageLocation { get; set; }


        /// <summary>
        /// Base Constructor for making a Place
        /// </summary>
        /// <param name="location"><see cref="location"/></param>
        /// <param name="name"><see cref="name"/></param>
        /// <param name="distance"><see cref="distance"/></param>
        /// <param name="types"><see cref="types"/></param>
        /// <param name="imageLocation"><see cref="imageLocation"/></param>
        public Place(GCoordinate location, string name, int? distance, string[] types, string imageLocation)
        {
            this.location = location;
            this.name = name;
            this.distance = distance;
            this.types = types;
            this.imageLocation = imageLocation;
        }
    }
}

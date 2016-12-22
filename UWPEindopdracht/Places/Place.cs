using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using Newtonsoft.Json;
using UWPEindopdracht.GPSConnections;

namespace UWPEindopdracht.Places
{
    public class Place
    {
        private const double DistanceOffset = 0.00000001;
       
        
        public double? Distance { get; set; }

        public HttpConnector.Priority SourcePriority = HttpConnector.Priority.Normal;
        public bool visited = false;

        /// <summary>
        /// Gcoordinate of the location of the place
        /// <see cref="GCoordinate"/>
        /// </summary>
        public GCoordinate Location { get; set; }
        /// <summary>
        /// Name of the place
        /// </summary>
        public string Name { get; set; }

        public GCoordinate[] Viewports { get; set; }


        public string Id { get; set; }

        /// <summary>
        /// Distance to the place (from the google places api)
        /// Warning! is optional and can be NULL
        /// <exception cref="NullReferenceException"> Exception when distance is not set</exception>
        /// </summary>
        //public double? Distance { get; set; }

        /// <summary>
        /// The types that the Place has like supermarket, point of interest, point of worship
        /// </summary>
        public string[] Types { get; set; }
        /// <summary>
        /// The image URL of the place location (Google places URL or other URL)
        /// Warning! is optional and can be NULL
        /// <exception cref="NullReferenceException"> Exception when image is not set</exception>
        /// </summary>
        public string ImageLocation { get; set; }
        /// <summary>
        /// The URL to the icon used for the place
        /// </summary>
        public string IconLink { get; set; }

        public string PlaceId { get; set; }

        [JsonIgnore]
        public MapIcon Icon { get; set; }

        public Place(HttpConnector.Priority sourcePriority, bool visited, double? distance, GCoordinate location, string name, GCoordinate[] viewports, string id, string[] types, string imageLocation, string iconLink, string placeId)
        {
            SourcePriority = sourcePriority;
            this.visited = visited;
            Distance = distance;
            Location = location;
            Name = name;
            Viewports = viewports;
            Id = id;
            Types = types;
            ImageLocation = imageLocation;
            IconLink = iconLink;
            PlaceId = placeId;
        }

       
        public bool IsCity()
        {
            if (Types == null)
                return false;
            List<string> types = new List<string>(Types);
            return types.Contains("locality") && types.Contains("political");
        }


        public bool IsSamePlace(Place place)
        {
            return Math.Abs(place.Location.lati - Location.lati) < DistanceOffset && Math.Abs(place.Location.longi - Location.longi) < DistanceOffset;
        }

        public void MergeInto(Place place)
        {
            if (SourcePriority > place.SourcePriority)
            {
                if(place.Name != null)
                    Name = place.Name;
                if(place.Id != null)
                    Id = place.Id;
                if(place.Distance != null)
                    Distance = place.Distance;
                if (place.Types.Length >= Types.Length)
                    Types = place.Types;
                if(place.ImageLocation != null)
                    ImageLocation = place.ImageLocation;
                if(place.IconLink != null)
                    IconLink = place.IconLink;
            }
            else
            {
                if (Name == null)
                    Name = place.Name;
                if (Distance == null)
                    Distance = place.Distance;
                if (Types == null || place.Types.Length >= Types.Length)
                    Types = place.Types;
                if (ImageLocation == null)
                    ImageLocation = place.ImageLocation;
                if (IconLink == null)
                    IconLink = place.IconLink;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPEindopdracht.DataConnections;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.Places;

namespace UWPEindopdracht
{
    class PlaceLoader
    {
        private static int _maxDistanceToPlace = 5000;
        public static async Task<List<Place>> GetPlaces(GCoordinate coordinate)
        {
            List<Place> places = await new GooglePlacesConnector().GetPlaces(_maxDistanceToPlace,coordinate);

            List<Place> wikiPlaces = await new WikipediaConnector().GetPlaces(_maxDistanceToPlace, coordinate);
            places = AddPlaces(places, wikiPlaces);
            return places;
        }

        public static List<Place> AddPlaces(List<Place> source, List<Place> additions )
        {
            foreach (var addedPlace in additions)
            {
                var existing = false;
                foreach (var sourcePlace in source)
                {
                    if (sourcePlace.IsSamePlace(addedPlace))
                    {
                        existing = true;
                        sourcePlace.MergeInto(addedPlace);
                    }
                }
                if (!existing)
                {
                    source.Add(addedPlace);
                }
            }
            foreach (var sour in source)
            {
                System.Diagnostics.Debug.WriteLine($"{sour.Name} - {sour.Distance}");
            }
            return source;
        }
        
    }
}

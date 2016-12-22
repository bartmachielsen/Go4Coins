using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using UWPEindopdracht.DataConnections;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.Places;

namespace UWPEindopdracht
{
    public class PlaceLoader
    {
        private static int _maxDistanceToPlace = 3000;
        public List<Place> Places = new List<Place>();
        private List<GCoordinate> _alreadyLoaded = new List<GCoordinate>();

        public PlaceLoader()
        {

        }

        public bool CheckIfLoadNeeded(GCoordinate coordinate)
        {
            return _alreadyLoaded.Find((gCoordinate => gCoordinate.Equals(coordinate))) == null;
        }

        public async Task LoadPlaces(GCoordinate position)
        {
            if (CheckIfLoadNeeded(position))
            {
                Places = AddPlaces(Places, await GetPlaces(position));
                Debug.WriteLine($"Loaded {Places.Count} points!");
                _alreadyLoaded.Add(position);
            }
        }

        private static async Task<List<Place>> GetPlaces(GCoordinate coordinate)
        {
            try
            {
                return AddPlaces(
                    await new GooglePlacesConnector().GetPlaces(_maxDistanceToPlace, coordinate), 
                    await new WikipediaConnector().GetPlaces(_maxDistanceToPlace, coordinate));
            }
            catch (ApiLimitReached)
            {
                await new MessageDialog("Api Limit is reached!", "Api Exception").ShowAsync();
            }
            catch (InvalidApiKeyException)
            {
                await new MessageDialog("Api key is invalid!", "Api Exception").ShowAsync();
            }
            catch (Exception)
            {
                // ignored
            }
            return new List<Place>();
        }

        private static List<Place> AddPlaces(List<Place> source, List<Place> additions )
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
            return source;
        }
        
    }
}

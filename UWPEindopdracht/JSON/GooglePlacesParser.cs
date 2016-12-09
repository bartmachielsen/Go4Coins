using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.Places;

namespace UWPEindopdracht.JSON
{
    class GooglePlacesParser
    {
        public static Place[] GetPlaces(string response)
        {
            /** BEGIN JSON
             * De totale json bestaat uit 
             * - html_attributions ( hoeven we niks mee te doen)
             * - next_page_token ( deze is nodig omdat er maar een beperkt aantal results kunnen in een json query dus je moet die next_page_token gebruiken om te kijken om de volgende pagina op te halen
             * - results (hierin staan alle resultaten en dit object is een JArray!)
             * - status (hierin staat of de query goed is of dat hij iets niet op kan halen!) --> moet OK zijn
            **/
            
            /**
             * DUS JE MOET DE JARRAY UIT RESULTS HALEN EN DOORLOPEN! DAARUIT KUN JE DE ONDERSTAANDE ONDERDELEN HALEN
             * een result query bevat de volgende elementen die voor ons van belang zijn!
             * - geometry ( bevat de locatie en viewport van het punt)
             *     - location wat de exacte locatie bevat
             *          -   lat voor de latitude
             *          -   lng voor de longitude
             * - icon (bevat de icon van het punt) in de vorm van een url
             * - name (naam van het object)
             * - photos
             *      -   photo_reference voor het later ophalen van een foto
             * - types wat een array is die zegt wat hij betekent
             * - vicinity is het adress of plaatsnaam in de vorm van een string
            **/
            dynamic json = JsonConvert.DeserializeObject(response);
            System.Diagnostics.Debug.WriteLine(response);
            var placesToSend = new List<Place>();
            var typeList = new List<string>();
            
            if (!(json is JObject))
            {
                System.Diagnostics.Debug.WriteLine(response);
                return null;
            }
            JObject jsonval = JObject.Parse(json);
            dynamic jsonplaces = jsonval;
            foreach (var jsonplace in jsonplaces)
            {
                var lati = jsonplace.geometry.location.lat;
                var longi = jsonplace.geometry.location.lng;

                foreach (var type in jsonplace.types)
                {
                    typeList.Add(type);
                }

                placesToSend.Add(new Place(
                    new GCoordinate(lati, longi), 
                    jsonplace.name, 
                    typeList.ToArray(), 
                    $"{lati}, {longi}", 
                    jsonplace.icon, 
                    jsonplace.reference)
                );
            }
            return placesToSend.ToArray();
        }
    }
}

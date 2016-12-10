using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UWPEindopdracht.GPSConnections;
using UWPEindopdracht.Places;

namespace UWPEindopdracht.JSON
{
    class GooglePlacesParser
    {
        public static string GetStatus(string response)
        {
            dynamic json = JsonConvert.DeserializeObject(response);
            string token = json.status;
            if (token == "" || token == " ")
                return null;
            return token;
        }
        public static string NextPage(string response)
        {
            dynamic json = JsonConvert.DeserializeObject(response);
            string token = json.next_page_token;
            if (token == "" || token == " ")
                return null;
            return token;
            
        }
        public static List<Place> GetPlaces(string response)
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
            var placesToSend = new List<Place>();
            
            
            if (!(json is JObject))
            {
                return null;
            }
            foreach (dynamic jsonplace in json.results)
            {
                var typeList = new List<string>();
                foreach (var type in jsonplace.types)
                {
                    typeList.Add((string)type);
                }
                placesToSend.Add(new Place
                {
                    Location = new GCoordinate((double)jsonplace.geometry.location.lat, 
                                                (double)jsonplace.geometry.location.lng),
                    Name = (string)jsonplace.name,
                    Types = typeList.ToArray(),
                    IconLink = jsonplace.icon,
                    ImageLocation = jsonplace.reference
                });
            }
            return placesToSend;
        }
    }
}

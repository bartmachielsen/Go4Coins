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
            // where is this one created ?
            Place[] placesToSend = null;

            // why these variables that are used in a lower piece ?
            var name = "";
            var lati = 0.0;
            var longi = 0.0;
            var reference = "";
            var icon = "";
            string[] types = null;
            var count = 0;
            
            if (!(json is JObject))
            {
                System.Diagnostics.Debug.WriteLine(response);
                return null;
            }
            // is not possible to parse because it is not a JArray!
            JArray jsonval = JArray.Parse(json);
            // why this dynamic and not a forloop with the created jarray?
            dynamic jsonplaces = jsonval;
            foreach (dynamic jsonplace in jsonplaces)
            {
                // why save temp and not immediately in constructor of place? (PLEASE NO EXTRA SAVE)
                name = jsonplace.name;
                reference = jsonplace.photos.photo_reference;
                lati = jsonplace.geometry.location.lat;
                longi = jsonplace.geometry.location.lng;
                icon = jsonplace.icon;
                // jsonplace.type.Count is not possible! replace with a forloop! 
                for (int i = 0; i < jsonplace.type.Count; i++)
                {
                    // types is null because never initialized
                    // jsonplace.type is not possible because it is a whole array that is saved everytime
                    types[i] = jsonplace.type;
                }
                if (types != null)
                {
                    // nullpointer because placestosend is null
                    placesToSend[count] = new Place(new GCoordinate(lati, longi), name, 0, types, $"{lati}, {longi}", icon, reference);
                    count++;
                }
                // why kill the whole loop if one item does not containt types ?
                else return null;
            }
            // why first check if object is null and if null return null ? bullshit extra code
            if (placesToSend != null)
            {
                return placesToSend;
            }
            else return null;
        }
    }
}

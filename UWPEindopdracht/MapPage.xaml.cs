using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using UWPEindopdracht.GPSConnections;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPEindopdracht
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page
    {
        
        public Geopoint location { get; set; }
        public MapIcon icon;
        public bool follow = false;



        public MapPage()
        {
            setLocation();

            this.InitializeComponent();
            
            var locator = new Geolocator() { DesiredAccuracyInMeters = 10, ReportInterval = 100};
            locator.PositionChanged += Locator_PositionChanged;
            
            mapControl.ZoomInteractionMode = MapInteractionMode.GestureAndControl;
            mapControl.ZoomLevel = 13;
            
        }

        private async void setLocation()
        {
            location = (await GPSHelper.getLocationOriginal()).Coordinate.Point;
            mapControl.Center = location;
            icon = new MapIcon();
            icon.Title = "You";
            icon.Location = location;
            mapControl.MapElements.Add(icon);
        }
        private async void Locator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                location = args.Position.Coordinate.Point;
                if(follow)
                    mapControl.Center = location;
                
                icon.Location = location;
                
                
            });
            
        }

        
      
       
    }

    
}

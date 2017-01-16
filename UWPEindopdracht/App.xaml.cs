using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using UWPEindopdracht.DataConnections;
using UWPEindopdracht.JSON;
using UWPEindopdracht.Multiplayer;

namespace UWPEindopdracht
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = false;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MapPage), e.Arguments);
                    //UploadAllRewardsInFolder("Disney");

                    // UPDATER IN NEW DATABASE
                    //await DownloadAllRewards();
                    //await UploadAllRewards();
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        private async Task DownloadAllRewards()
        {
            var db = new RestDBConnector
            {
                Host = "https://uwpeindopdracht-429b.restdb.io/rest",
                ApiKey = "711dc584f7d33bf508b643a165c95bc9a4129"
            };
            List<Reward> rewards = await db.GetRewards();

            Windows.Storage.StorageFolder storageFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile =
                await storageFolder.CreateFileAsync("rewards.json",
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);
            System.Diagnostics.Debug.WriteLine(sampleFile.Path);
            System.Diagnostics.Debug.WriteLine(RestDBHelper.ConvertRewards(rewards));
            await Windows.Storage.FileIO.WriteTextAsync(sampleFile, RestDBHelper.ConvertRewards(rewards));
        }

        private async Task UploadAllRewards()
        {
            Windows.Storage.StorageFolder storageFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile =
                await storageFolder.CreateFileAsync("rewards.json",
                    Windows.Storage.CreationCollisionOption.OpenIfExists);
            if (sampleFile.IsAvailable)
            {
                string content = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);
                var db = new RestDBConnector();
                List<Reward> rewards = RestDBHelper.GetRewards(content);
                foreach (var reward in rewards)
                {
                    System.Diagnostics.Debug.WriteLine("UPLOADING " + reward.Name);
                    await db.UploadReward(reward);
                    await Task.Delay(1000);
                }
            }

        }

        private async Task UploadAllRewardsInFolder(string folderName)
        {
            var db = new RestDBConnector();
            StorageFolder InstallationFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync(@"Assets\Reward\"+folderName);

            foreach (var file in await InstallationFolder.GetFilesAsync())
            {
                string fileName = file.Name;
                List<int> indexes = new List<int>();
                var chararray = fileName.ToCharArray();
                for (int i = 0; i < chararray.Length; i++)
                {
                    var character = chararray[i];
                    if (("" + character) == ("" + character).ToUpper() && character != '.')
                    {
                        indexes.Add(i);
                    }
                }
                string newName = fileName;
                if (indexes.Count > 0)
                    foreach (var index in indexes)
                    {
                        newName = fileName.Insert(index, " ");
                    }
                    
                newName = newName.Replace(".png", "");
                newName = (newName[0] + "").ToUpper() + newName.Substring(1);
                Reward reward = new Reward(null, newName, 
                    @"Assets/Reward/" + folderName + @"/"+ fileName, 
                    @"Assets/Reward/" + folderName + @"Grey/" + fileName.Replace(".png","") + "Grey.png",
                    newName,
                    folderName,
                    0);
                System.Diagnostics.Debug.WriteLine(reward.Name);
                await db.UploadReward(reward);
                await Task.Delay(1000);
            }

        }

    }
}

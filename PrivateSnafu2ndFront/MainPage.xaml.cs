using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources.Core;
using Windows.Data.Json;
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
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PrivateSnafu2ndFront
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        // The movie list, and last selected movie 
        static public ObservableCollection<MovieData> GlobalMovieList = new ObservableCollection<MovieData>();
        static public int PersistSelectedIndex = 0;

        // Toggle for setting focus
        private bool bAfterLoaded = false;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            this.LayoutUpdated += MainPage_LayoutUpdated;
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            bAfterLoaded = !bAfterLoaded;

            // Remove the UI from the title bar if in-app back stack is empty.
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Collapsed;

            // Get movie list from resources (will load from current culture resource branch)
            ResourceMap rmap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");
            var ctx = ResourceContext.GetForCurrentView();

            // Dynamically load the new background image 
            // ( It would be possible to cycle through a number of different images! )
            var backDrop = rmap.GetValue("MAIN_PAGE_BACKDROP_1", ctx).ValueAsString;

            ImageBrush ib = new ImageBrush();
            Image image = new Image();
            image.Source = new BitmapImage( new Uri(backDrop));

            ib.ImageSource = image.Source;
            ib.Opacity = .2;
            MainPage_Grid.Background = ib;

            // Now load our About and Contact info
            AboutPaneText.Text = rmap.GetValue("ABOUT", ctx).ValueAsString;
            ContactPaneText.Text = rmap.GetValue("CONTACT", ctx).ValueAsString;

            // The movie list is a specific JSON file tailored to each language/culture
            string strMovieList = rmap.GetValue("MOVIE_LIST", ctx).ValueAsString;

            // Empty the old list, then asynchronously wait to get the new
            GlobalMovieList.Clear();
            await FillMovieList(strMovieList);

            // If no fatal errors, show the movies to the user
            if (false == cannotContinueState)
            {
                MovieGridView.ItemsSource = GlobalMovieList;
                MovieGridView.SelectedIndex = PersistSelectedIndex;
                MovieGridView.ScrollIntoView(MovieGridView.SelectedItem);
            }
            else
            {
                /// Display any error msgs (from "errors.resw")
                ResourceMap rErrorMap = ResourceManager.Current.MainResourceMap.GetSubtree("errors");

                HelpTitle.Text  = rErrorMap.GetValue("FATAL_ERROR", ctx).ValueAsString;
                HelpMsg.Text    = rErrorMap.GetValue(cannotContinueMsgGetSet, ctx).ValueAsString;
                HelpExcept.Text = cannotContinueError.ToString();
            }
        }

        private void MainPage_LayoutUpdated(object sender, object e)
        {
            // Set focus to the video player
            if (bAfterLoaded)
            {
                bool b = MovieGridView.Focus(FocusState.Programmatic);
                bAfterLoaded = !bAfterLoaded;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        // Sometimes Right Click has e.AddedItems.Count == 0, so check for that
        // Note that the e param has the selection as the first AddedItems 
        private void MovieGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                // Update the onscreen detail text
                SelectedTitle.Text = ((MovieData)e.AddedItems[0]).Title;
                SelectedReleaseYear.Text = ((MovieData)e.AddedItems[0]).ReleaseDate;
                SelectedAbout.Text = ((MovieData)e.AddedItems[0]).About;
            }
        }

        /// /////////////////////////////////////////////////////////////////////////////        
        /// "Click", "double click" and "Enter" key can launch the movie
        private void MovieGridView_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                StartMovie();
            }
        }
        private void MovieGridView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            StartMovie();
        }
        private void MovieGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartMovie();
        }
        /// /////////////////////////////////////////////////////////////////////////////

        private void StartMovie()
        {
            // The selected item index is a property of the GridView container
            // Persist the selected movie index (so it is available after we refresh the movie list)
            PersistSelectedIndex = MovieGridView.SelectedIndex;
            Uri SelectedShort;

            // The AltUrl is a movie physically bundled with this app, which allows
            //    demo playback even without the internet
            // 
            if (String.IsNullOrEmpty((GlobalMovieList[MovieGridView.SelectedIndex]).AltUrl))
            {
                SelectedShort = new Uri((GlobalMovieList[MovieGridView.SelectedIndex]).Url);
            }
            else
            {
                SelectedShort = new Uri((GlobalMovieList[MovieGridView.SelectedIndex]).AltUrl);
            }

            // Send the Url to play to the MoviePlayer page    
            Frame.Navigate(typeof(PlayerPage), SelectedShort);
        }

        /// <summary>
        // Read the file "SnafuMovies.json" bundled with the app resources into a JSON array
        /// 
        /// JSON - Parse JSON Strings in Windows Runtime Components
        /// https://msdn.microsoft.com/en-us/magazine/dn198241.aspx
        /// </summary>
        private static async Task FillMovieList(string strMovieList)
        {
            string rawMovieList = null;
            StorageFolder folderAssets = null;
            StorageFile fileMovies = null;

            try
            {
                // Movie list is bundled into AppX Assets as "Assets\SnafuMovies.XX.json" (where .XX is the language)
                folderAssets = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("assets");
                if (folderAssets != null)
                {
                    fileMovies = await folderAssets.GetFileAsync(strMovieList);
                    if (fileMovies != null)
                    {
                        rawMovieList = await FileIO.ReadTextAsync(fileMovies);
                    }
                }
            }
            catch (Exception e)
            {
                // Either the AppX Assets folder or the movie file list could not be found
                // The app cannot continue after either error
                cannotContinueState = true;
                cannotContinueMsgGetSet = "NO_MOVIE_LIST";
                cannotContinueError = e;
                return;
            }

            if (String.IsNullOrEmpty(rawMovieList))
            {
                cannotContinueState = true;
                cannotContinueMsgGetSet = "CORRUPT_MOVIE_LIST";
                return;
            }

            JsonArray jsonMovies = new JsonArray();
            JsonArray.TryParse(rawMovieList, out jsonMovies);
            int size = jsonMovies.Count;
            if (size == 0)
            {
                cannotContinueState = true;
                cannotContinueMsgGetSet = "EMPTY_MOVIE_LIST";
                return;
            }

            JsonObject jObj;
            string title, year, about, thumbnail, source, altSource;
            title = year = about = thumbnail = source = altSource = null;
            for (uint n = 0; n != size; n++)
            {
                jObj = jsonMovies.GetObjectAt(n);
                if (jObj.ContainsKey("TITLE"))
                {
                    title = jObj.GetNamedString("TITLE");
                }
                if (jObj.ContainsKey("YEAR"))
                {
                    year = jObj.GetNamedString("YEAR");
                }
                if (jObj.ContainsKey("ABOUT"))
                {
                    about = jObj.GetNamedString("ABOUT");
                }
                if (jObj.ContainsKey("THUMBNAIL"))
                {
                    thumbnail = jObj.GetNamedString("THUMBNAIL");
                }
                if (jObj.ContainsKey("MOVIE"))
                {
                    source = jObj.GetNamedString("MOVIE");
                }
                if (jObj.ContainsKey("ALT-MOVIE"))
                {
                    altSource = jObj.GetNamedString("ALT-MOVIE");
                }
                // Add the movies to the MovieList
                GlobalMovieList.Add(new MovieData
                (
                    title, year, about, thumbnail, source, altSource
                ));
            }
            int result = GlobalMovieList.Count();
            return;
        } /// private static async void InitJSONMovie()


        /// <summary>
        ///  Settings - Hamburger, "About" and "Contact" buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the SplitView pane
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void IconsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Toggle the SplitView pane
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;

            if (AboutListBoxItem.IsSelected)
            {
                /// $$ Webpage
            }
            else if (ContactListBoxItem.IsSelected)
            {
                /// $$ Email
            }
        }

        private void ContactButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Toggle the SplitView pane
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }
        private async void ContactBlock_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ResourceContext ctx = new ResourceContext();
            ResourceMap rmap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");

            // Add language/culture text
            var MailtoUrl = rmap.GetValue("MAIL_TO", ctx).ValueAsString;
            var UriContactSnafu = new Uri(MailtoUrl);
            var success = await Windows.System.Launcher.LaunchUriAsync(UriContactSnafu);
            if (success)
            {
                // URI launched
            }
            else
            {
                // URI did not launch
            }
        }

        private void AboutBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Toggle the SplitView pane
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }
        private async void AboutBlock_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ResourceContext ctx = new ResourceContext();
            ResourceMap rmap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");

            // Add language/culture text
            var AboutUrl = rmap.GetValue("ABOUT_URL", ctx).ValueAsString;

            var UriAbout = new Uri(AboutUrl);
            var success = await Windows.System.Launcher.LaunchUriAsync(UriAbout);
            if (success)
            {
                // URI launched
            }
            else
            {
                // URI did not launch
            }
        }

        /// <summary>
        /// void SwitchLanguage()
        /// Cycle through the app's supported languages
        /// </summary>
        private void SwitchLanguage()
        {
            // Get the current language(s)
            var ctx = ResourceContext.GetForCurrentView();
            string[] langs = new string[ctx.Languages.Count];
            langs = ctx.Languages.ToArray();

            // Get the next App supported language and set the primary language to that
            App.setNextLanguage();
            langs[0] = App.getCurrentLanguage();
            ctx.Languages = langs;

            // Return to home page, refreshing everything on this page.
            Frame.Navigate(typeof(MainPage));
        }

        /// The "Secret" button is invisible just below the last Hamburger stack button
        /// It's used to switch the primary language 
        private void Secret_Button_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            SwitchLanguage();
            e.Handled = true;
        }
        private void SecretButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchLanguage();
        }

        // Error notifications
        private static bool cannotContinueState = false;
        private static String cannotContinueMsg = "";
        private static Exception cannotContinueError = null;
        public static string cannotContinueMsgGetSet
        {
            get
            {
                return cannotContinueMsg;
            }
            set
            {
                cannotContinueMsg = value;
            }
        }

    } /// public sealed partial class MainPage : Page
} /// namespace BettyB


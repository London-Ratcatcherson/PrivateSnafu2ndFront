using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PrivateSnafu2ndFront
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayerPage : Page
    {
        private bool bAfterLoaded = false;
        public PlayerPage()
        {
            this.InitializeComponent();
            this.Loaded += PlayerPage_Loaded;
            this.LayoutUpdated += PlayerPage_LayoutUpdated;
        }

        private void PlayerPage_Loaded(object sender, RoutedEventArgs e)
        {
            bAfterLoaded = !bAfterLoaded;

            // Get the current language being used and insert into context
            // (If we changed the current language in the home page, we
            // (  need to use that, and not the global Windows context)
            var ctx = ResourceContext.GetForCurrentView();
            string[] langs = new string[ctx.Languages.Count];
            langs = ctx.Languages.ToArray();

            // Get the language set on MainPage and set this local context
            langs[0] = App.getCurrentLanguage();
            ctx.Languages = langs;

            // Get the strings for our controls
            ResourceMap rmap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");

            // Set the top button bar and pull the labels from the current Culture (language)
            commandContentText.Text = rmap.GetValue("CMDBAR_TEXT", ctx).ValueAsString;
            Home.Label = rmap.GetValue("HOME", ctx).ValueAsString;
            FullScreenToggle.Label = rmap.GetValue("FULL_SCREEN", ctx).ValueAsString;
            Vol_Louder.Label = rmap.GetValue("LOUDER", ctx).ValueAsString;
            Vol_Softer.Label = rmap.GetValue("SOFTER", ctx).ValueAsString;

            // And the bottom button bar
            Previous.Label = rmap.GetValue("REVERSE", ctx).ValueAsString;
            Play.Label = rmap.GetValue("PLAY", ctx).ValueAsString;
            Pause.Label = rmap.GetValue("PAUSE", ctx).ValueAsString;
            Stop.Label = rmap.GetValue("STOP", ctx).ValueAsString;
            Next.Label = rmap.GetValue("FORWARD", ctx).ValueAsString;
        }

        private void PlayerPage_LayoutUpdated(object sender, object e)
            {
            // Set focus to the Full Screen Toggle button
            if (bAfterLoaded)
            {
                bool b = FullScreenToggle.Focus(FocusState.Programmatic);
                bAfterLoaded = !bAfterLoaded;
            }
        }


        /////////////////////////////////////////////////////////////////////////////////
        // Disable screen dimming while movie plays, re-enable after
        private DisplayRequest dispRequest = null;
        private long dispRequestCount = 0L;

        private void screenDimOff()
        {
            // Disable any DisplayRequests that are active
            if (null != dispRequest)
            {
                while (dispRequestCount > 0)
                {
                    dispRequest.RequestRelease();
                    dispRequestCount--;
                }
            }
        }
        private void screenDimOn()
        {
            // Enable the DisplayRequest and increment its counter
            if (null != dispRequest)
            {
                dispRequest.RequestActive();
                dispRequestCount++;
            }
        }


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack)
            {
                // Show UI in title bar if opted-in and in-app backstack is not empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
            }
            // We only reach this page when a movie is selected to play, 
            //   so no error checking is done for the e.Parameter.
            object inbound = e.Parameter.GetType();
            {
                // Create a new instance of the DisplayRequest object and turn off screen dimming
                if (null == dispRequest)
                {
                    dispRequest = new Windows.System.Display.DisplayRequest();
                    screenDimOn();
                }
                // Set the movie source to the Uri passed in
                // The MediaElement is set to AutoPlay, so now we're buffering then playing
                videoMediaElement.Source = (System.Uri)e.Parameter;
            }
        }
        /////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////////////////////
        /// Page.TopAppBar
        /// 
        /// <summary>
        ///  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        // Handle the Back and Home buttons by returning to MainPage
        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;

            // Disable any DisplayRequests that are active
            screenDimOff();

            // Navigate back if possible, and if the event has not already been handled
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        // Release all the DisplayRequest calls to halt screen dimming, then return to Main page
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            // Disable any DisplayRequests that are active
            screenDimOff();

            // Return to home page, freeing everything on this page.
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
        /// Page.TopAppBar
        /////////////////////////////////////////////////////////////////////////////////

        
        /////////////////////////////////////////////////////////////////////////////////
        /// Media control button event handlers

        /// Play button event handler
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            // Disable screen dimming
            screenDimOn();
            // Resume normal speed playback
            if (videoMediaElement.DefaultPlaybackRate != 1)
            {
                videoMediaElement.DefaultPlaybackRate = 1.0;
            }
            videoMediaElement.Play();
        }
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            // Disable any DisplayRequests that are active
            screenDimOff();
            videoMediaElement.Pause();
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            // Disable any DisplayRequests that are active
            screenDimOff();
            videoMediaElement.Stop();
        }
        // Fast Forward at 2x or 4x
        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (videoMediaElement.DefaultPlaybackRate == 1.0)
            {
                videoMediaElement.DefaultPlaybackRate = 2.0;
            }
            else if (videoMediaElement.DefaultPlaybackRate == 2.0)
            {
                videoMediaElement.DefaultPlaybackRate = 4.0;
            }
            else if (videoMediaElement.DefaultPlaybackRate == 4.0)
            {
                // Quad forward is our max
            }
            else if (videoMediaElement.DefaultPlaybackRate == -1.0)
            {
                videoMediaElement.DefaultPlaybackRate = 1.0;
            }
            else if (videoMediaElement.DefaultPlaybackRate == -2.0)
            {
                videoMediaElement.DefaultPlaybackRate = -1.0;
            }
            else if (videoMediaElement.DefaultPlaybackRate == -4.0)
            {
                videoMediaElement.DefaultPlaybackRate = -2.0;
            }
            videoMediaElement.Play();
        }
        // Reverse at -2x or -4x
        private void Reverse_Click(object sender, RoutedEventArgs e)
        {
            if (videoMediaElement.DefaultPlaybackRate == 1.0)
            {
                videoMediaElement.DefaultPlaybackRate = -1.0;
            }
            else if (videoMediaElement.DefaultPlaybackRate == 2.0)
            {
                videoMediaElement.DefaultPlaybackRate = 1.0;
            }
            else if (videoMediaElement.DefaultPlaybackRate == 4.0)
            {
                videoMediaElement.DefaultPlaybackRate = 2.0;
            }
            else if (videoMediaElement.DefaultPlaybackRate == -1.0)
            {
                videoMediaElement.DefaultPlaybackRate = -2.0;
            }
            else if (videoMediaElement.DefaultPlaybackRate == -2.0)
            {
                videoMediaElement.DefaultPlaybackRate = -4.0;
            }
            else if (videoMediaElement.DefaultPlaybackRate == -4.0)
            {
                // Quad reverse is our max
            }
            videoMediaElement.Play();
        }

        // Lower volume
        private void VolumeDown_Click(object sender, RoutedEventArgs e)
        {
            if (videoMediaElement.IsMuted)
            {
                videoMediaElement.IsMuted = false;
            }
            if (videoMediaElement.Volume > 0)
            {
                videoMediaElement.Volume -= .1;
            }
        }
        // Raise volume
        private void VolumeUp_Click(object sender, RoutedEventArgs e)
        {
            if (videoMediaElement.IsMuted)
            {
                videoMediaElement.IsMuted = false;
            }
            if (videoMediaElement.Volume < 1)
            {
                videoMediaElement.Volume += .1;
            }
        }
        // No mute button. Keeping event for updates
        private void Mute_Click(object sender, RoutedEventArgs e)
        {
            videoMediaElement.IsMuted = !videoMediaElement.IsMuted;
        }

        /// Media control button event handlers
        /////////////////////////////////////////////////////////////////////////////////


        /////////////////////////////////////////////////////////////////////////////////
        /// Handle Fullscreen toggling. Fullscreen is default
        /// 
        private bool _isFullscreenToggle = true;
        public bool IsFullscreen
        {
            get { return _isFullscreenToggle; }
            set { _isFullscreenToggle = value; }
        }

        // Switch between full screen and NaturalVideo size
        private void FullscreenToggle()
        {
            if (this.IsFullscreen)
            {
                // Set the video size to the source video size
                videoMediaElement.Width = videoMediaElement.NaturalVideoWidth;
                videoMediaElement.Height = videoMediaElement.NaturalVideoHeight;
            }
            else
            {
                // Set the video size to the size of the current window
                videoMediaElement.Width = Window.Current.Bounds.Width;
                videoMediaElement.Height = Window.Current.Bounds.Height;
            }
            // Flip toggle to the other state
            this.IsFullscreen = !this.IsFullscreen;
        }
        // Route the Full screen button event
        private void FullScreenToggle_Click(object sender, RoutedEventArgs e)
        {
            FullscreenToggle();
        }
        /// End of "Handle Fullscreen toggling"
        /////////////////////////////////////////////////////////////////////////////////


        /////////////////////////////////////////////////////////////////////////////////
        /// Sync slider to media length
        /// Currently, the slider is "passive" (ie, user interaction does NOT alter movie stream)
        /// 
        private double SliderFrequency(TimeSpan timevalue)
        {
            double absvalue = (int)Math.Round(timevalue.TotalSeconds, MidpointRounding.AwayFromZero);
            double stepfrequency = (int)(Math.Round(absvalue / 100));

            if (timevalue.TotalMinutes >= 10 && timevalue.TotalMinutes < 30)
            {
                stepfrequency = 10;
            }
            else if (timevalue.TotalMinutes >= 30 && timevalue.TotalMinutes < 60)
            {
                stepfrequency = 30;
            }
            else if (timevalue.TotalHours >= 1)
            {
                stepfrequency = 60;
            }

            if (stepfrequency == 0)
            {
                stepfrequency += 1;
            }
            if (stepfrequency == 1)
            {
                stepfrequency = absvalue / 100;
            }
            return stepfrequency;
        }
        /// End of "Sync slider to media length"

        /// Create a DispatcherTimer to update the media played slider
        /// 
        private DispatcherTimer _timer;
        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(timelineSlider.StepFrequency);
            StartTimer();
        }

        private void _timer_Tick(object sender, object e)
        {
            if (!_sliderpressed)
            {
                timelineSlider.Value = videoMediaElement.Position.TotalSeconds;
            }
        }

        private void StartTimer()
        {
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }
        private void StopTimer()
        {
            _timer.Stop();
            _timer.Tick -= _timer_Tick;
        }
        /// End of "Create a DispatcherTimer"
        /////////////////////////////////////////////////////////////////////////////////

        
        /////////////////////////////////////////////////////////////////////////////////
        /// Media Event handlers
        /// 
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            timelineSlider.ValueChanged += timelineSlider_ValueChanged;

            PointerEventHandler pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            timelineSlider.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);
            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);
            timelineSlider.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        }

        void videoElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            double absvalue = (int)Math.Round(
                videoMediaElement.NaturalDuration.TimeSpan.TotalSeconds, MidpointRounding.AwayFromZero);

            timelineSlider.Maximum = absvalue;
            timelineSlider.StepFrequency = SliderFrequency(videoMediaElement.NaturalDuration.TimeSpan);

            SetupTimer();

            // Helper method to populate the combobox with audio tracks
            // Currently unused and commented out
            //
            // PopulateAudioTracks(videoMediaElement, cbAudioTracks);
        }

        private bool _sliderpressed = false;

        void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _sliderpressed = true;
        }

        void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            videoMediaElement.Position = TimeSpan.FromSeconds(timelineSlider.Value);
            _sliderpressed = false;
        }

        void timelineSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (!_sliderpressed)
            {
                videoMediaElement.Position = TimeSpan.FromSeconds(e.NewValue);
            }
        }

        private TimeSpan CurrentTimePlayed;

        void videoMediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (videoMediaElement.CurrentState)
            {
                case MediaElementState.Closed:
                    // Screen rotation in simulator will halt playback and set Media to Closed
                    if (FailCount > 0)
                    {
                        // Save the current playback position
                        CurrentTimePlayed = videoMediaElement.Position;

                        // Reset media by setting the source
                        videoMediaElement.Source = videoMediaElement.Source;
                    }
                    break;
                case MediaElementState.Opening:
                    // If we're Opening and there's been failures, clear fail counter and set playback position to the last saved
                    if (FailCount > 0)
                    {
                        // Reset FailCount
                        FailCount = 0;

                        // Reset position to last saved
                        videoMediaElement.Position = CurrentTimePlayed;
                    }
                    break;
                case MediaElementState.Buffering:
                    break;
                case MediaElementState.Playing:
                    {
                        if (_sliderpressed)
                        {
                            _timer.Stop();
                        }
                        else
                        {
                            _timer.Start();
                        }
                    }
                    break;
                case MediaElementState.Paused:
                    {
                        _timer.Stop();
                    }
                    break;
                case MediaElementState.Stopped:
                    {
                        _timer.Stop();
                        timelineSlider.Value = 0;
                    }
                    break;
            }
        }

        // Movie has finished. Put up a cute final clip
        void videoMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            StopTimer();
            timelineSlider.Value = 0.0;

            videoMediaElement.Source = new Uri("ms-appx:///assets/TheEnd.jpg");
        }

        static int FailCount = 0;

        private void videoMediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // Get HRESULT from event args
            string strResultMsg = GetHresultFromErrorMessage(e);

            // TODO Handle media failed event
            // When display rotates, we get error 0xC00D4E86 MF_E_AUDIO_PLAYBACK_DEVICE_INVALIDATED
            // "The audio playback device is no longer present."
            // mferror.h
            //

            FailCount++;
        }

        private string GetHresultFromErrorMessage(ExceptionRoutedEventArgs e)
        {
            String hr = String.Empty;
            String token = "HRESULT - ";
            const int hrLength = 10;        // eg "0xFFFFFFFF"

            int tokenPos = e.ErrorMessage.IndexOf(token, StringComparison.Ordinal);
            if (tokenPos != -1)
            {
                hr = e.ErrorMessage.Substring(tokenPos + token.Length, hrLength);
            }
            return hr;
        }
        /// End of "Media Event handlers"
        /////////////////////////////////////////////////////////////////////////////////


        /////////////////////////////////////////////////////////////////////////////////
        /// Handle multiple audio tracks
        ///   Our media source only has a single audio track (US-EN) so we don't use this
        /// cbAudioTracks_SelectionChanged() depends on the cbAudioTracks control, which was removed.
        /// 
        //private void cbAudioTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    videoMediaElement.AudioStreamIndex = cbAudioTracks.SelectedIndex;
        //}
        private void PopulateAudioTracks(MediaElement media, ComboBox audioSelection)
        {
            if (media.AudioStreamCount > 0)
            {
                for (int index = 0; index < media.AudioStreamCount; index++)
                {
                    ComboBoxItem track = new ComboBoxItem();
                    track.Content = media.GetAudioStreamLanguage(index);
                    audioSelection.Items.Add(track);
                }
            }
        }

        private void cbAudioTracks_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
        /// End of "Handle multiple audio tracks"
        /////////////////////////////////////////////////////////////////////////////////

        /// Keyboard events could be handled here.
        private void grid_BigScreen_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                // Use Escape for something
            }
            e.Handled = true;
        }

        private void grid_BigScreen_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                // Use Escape for something
            }
            e.Handled = true;

        }
    } /// public sealed partial class PlayerPage : Page
} /// namespace BettyB


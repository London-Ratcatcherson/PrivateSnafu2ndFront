using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace PrivateSnafu2ndFront
{
    public class MovieData
    {
        public string Title { get; set; }
        public string ReleaseDate { get; set; }
        public string About { get; set; }
        public ImageSource Thumbnail { get; set; }
        public string Url { get; set; }
        public string AltUrl { get; set; }

        // Full constructor
        public MovieData(string TitleSrc, string ReleaseDateSrc, string AboutSrc, string ThumbnailSrc, string UrlSrc, string NotesSrc)
        {
            Title = TitleSrc;
            ReleaseDate = ReleaseDateSrc;
            About = AboutSrc;
            try
            {
                Thumbnail = new BitmapImage(new Uri(ThumbnailSrc));
            }
            catch (System.IO.FileNotFoundException)
            {
                // Load a default image for any invalid thumbnail
                // $$ The path to the bitmap is incorrect, but we avoid fault and just get a blank thumbnail so OK for now
                Thumbnail = new BitmapImage(new Uri("ms-appx:///assets/PrivateSnafuTitle150x150.scale-100.png"));
            }
            Url = UrlSrc;
            AltUrl = NotesSrc;
        }
    } // public class MovieData
}



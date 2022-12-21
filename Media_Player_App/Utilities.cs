using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Media_Player_App
{
    public class Utilities
    {
        public const string PlayListFolder = "PlayList";
        public const string RecentlyPlayed = "RecentlyPlayed";
        public static BitmapImage covertStringtoBitmapImage(string url)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(url, UriKind.Relative);
            bitmap.EndInit();

            return bitmap;
        }
        public static DateTime DoubleToDateTime(double dDateTime)
        {
            return DateTime.FromOADate(dDateTime);
        }
    }
}

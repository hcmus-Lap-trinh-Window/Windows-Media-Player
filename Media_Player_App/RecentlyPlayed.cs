using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Player_App
{
    public class RecentlyPlayed
    {
        public ObservableCollection<Media>? _PlayLists { get; set; }
        public double? CurrentPosition { get; set; }
        public Media? CurrentMedia { get; set; }
        public int CurrentPlayingIndex { get; set; }
        public double? CurrentVolume { get; set; }
        public bool? IsMediaPlaying { get; set; }
        public bool? IsMediaShuffle { get; set; }
    }
}

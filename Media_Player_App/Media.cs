using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Media_Player_App
{
    public class Media : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string? Name { get; set; }
        public string? Singer { get; set; }
        public string? ImagePath { get; set; }
        public Uri? FullPath { get; set; }
        public string? RunTime { get; set; }
        public string? EndTime { get; set; }
    }
}

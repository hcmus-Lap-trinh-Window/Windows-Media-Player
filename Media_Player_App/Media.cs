using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
        public BitmapImage? Image { get; set; }
        public Media() { }
        public Media(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                throw new Exception($"The file {filePath} doesn't exist.");
            }
            var mediaFile = TagLib.File.Create(filePath);

            this.Name = mediaFile.Tag.Title ?? fileInfo.Name;       //  Lấy tên file
            this.FullPath = new Uri(filePath);

            #region Lấy thông tin ca sĩ

            var aritistList = mediaFile.Tag.Artists;
            if (aritistList == null || aritistList.Length == 0)
            {
                this.Singer = "Unknown";
            }
            else
            {
                this.Singer = String.Join(";", aritistList);
            }

            #endregion
            // Get image from media file
            var firstPicture = mediaFile.Tag.Pictures.FirstOrDefault();
            if (firstPicture != null)
            {
                byte[] imageData = firstPicture.Data.Data;
                var image = new BitmapImage();
                using (var mem = new MemoryStream(imageData))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
                this.Image = image;
            }
            else
            {
                this.Image = new BitmapImage(new Uri(@"Images\musical-note.png", UriKind.RelativeOrAbsolute));
            }
        }
    }
}

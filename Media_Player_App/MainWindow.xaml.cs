using HandyControl.Data;
using HandyControl.Tools.Extension;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Media_Player_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isMediaPlaying = false;
        private bool isMediaSuffle = false;
        private bool isMediaNewFile = false;
        private Media CurrentMedia = null;
        private ObservableCollection<Media> _PlayLists;
        private ObservableCollection<String> _PlayListComboBox;
        private const string PlayListFolder = "PlayList";

        public MainWindow()
        {
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // init flag for tracking media player's state
            isMediaPlaying = false;
            isMediaSuffle = true;

            // init playlist
            _PlayLists = new ObservableCollection<Media>();
            Playlists.ItemsSource = _PlayLists;
            Playlists.SelectionMode = System.Windows.Controls.SelectionMode.Single;

            // init playlist combo box
            _PlayListComboBox = new ObservableCollection<string>(GetAllJsonFile(PlayListFolder).Select(c => c.getFileName()).OrderBy(c => c).ToList());
            PlaylistComboBox.ItemsSource = _PlayListComboBox;
        }

        private void New_File_Button_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                var _currentPlaying = screen.FileName;
                FileInfo fileInfo = new FileInfo(_currentPlaying);
                Media newMedia = new Media()
                {
                    Name = fileInfo.Name,
                    Singer = "Truong Cong Thanh",
                    ImagePath = Directory.GetCurrentDirectory() + @"/Images/thanh.png",
                    FullPath = new Uri(_currentPlaying),
                };
                var isDuplicate = _PlayLists.Where(c => c.FullPath == newMedia.FullPath && c.Name == newMedia.Name).ToArray();
                if (isDuplicate == null || isDuplicate.Length <= 0)
                {
                    // we have the flow: -> load file -> if not duplicate -> add to playlist -> play the newest song, update button in UI -> set CurrentMedia = new song
                    // -> set selected item in playlist = new song
                    _PlayLists.Add(newMedia);
                    media.Source = new Uri(_currentPlaying, UriKind.Absolute);
                    #region set change in UI
                    // if add new song, play it, no check state
                    media.Play();
                    isMediaPlaying = true;
                    UpdatePlayButton();
                    #endregion
                    CurrentMedia = newMedia;
                    #region set selected item in playlist
                    Playlists.SelectedItem = newMedia;
                    #endregion
                }
            }
        }

        private void PlaylistComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var playListSelected = PlaylistComboBox.SelectedValue;
                if (playListSelected == null)
                {
                    return;
                }
                var playListDirectory = Directory.GetCurrentDirectory() + $"\\{PlayListFolder}\\{playListSelected}.json";
                var playListContent = File.ReadAllText(playListDirectory);
                if (playListContent != null)
                {
                    var jsonToPlayList = JsonSerializer.Deserialize<ObservableCollection<Media>>(playListContent);
                    if (jsonToPlayList != null)
                    {
                        _PlayLists = new ObservableCollection<Media>();
                        foreach (var media in jsonToPlayList)
                        {
                            _PlayLists.Add(media);
                        }
                    }
                }
                Playlists.ItemsSource = _PlayLists;
                AddPlaylist.Text = playListSelected.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(MessageType.Error, "Errors", String.Format("{0}. {1}", ex.Message, ex.InnerException?.Message));
            }
        }

        private void Clear_All_Files_In_Playlist_Btn_Click(object sender, RoutedEventArgs e)
        {
            _PlayLists.Clear();
            media.Source = null;
        }

        private void Add_Playlist_Button_CLick(object sender, RoutedEventArgs e)
        {
            try
            {
                var playListName = AddPlaylist.Text;
                if (!string.IsNullOrWhiteSpace(playListName))
                {
                    if (_PlayLists != null && _PlayLists.Count > 0)
                    {
                        var playListFileName = playListName + ".json";
                        var playListDirectory = Directory.GetCurrentDirectory() + $@"\\{PlayListFolder}\\";
                        var playListJson = JsonSerializer.Serialize(_PlayLists);
                        SaveJson(playListDirectory + playListFileName, playListJson);
                        if (!_PlayListComboBox.Contains(playListName))
                        {
                            _PlayListComboBox.Add(playListName);
                        }
                    }
                    else
                    {
                        throw new Exception("Let's choose media file for your playlist firstly!");
                    }
                }
                else
                {
                    throw new Exception("Please name your playlist");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MessageType.Error, "Errors", String.Format("{0}. {1}", ex.Message, ex.InnerException?.Message));
            }
        }
        private void SaveJson(string filePath, string data, bool isOverride = false)
        {
            try
            {
                var folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(filePath));
                if (folderName == null)
                {
                    throw new Exception("Folder name not existed");
                }
                var listFile = GetAllJsonFile(folderName);
                var fileName = System.IO.Path.GetFileName(filePath);
                if (!listFile.Contains(fileName, StringComparer.OrdinalIgnoreCase) || isOverride == true)
                {
                    File.WriteAllText(filePath, data);
                    HandyControl.Controls.MessageBox.Show(new MessageBoxInfo()
                    {
                        Message = "Save PlayList Successfully",
                        Caption = "Save PlayList",
                        Button = MessageBoxButton.OK,
                    });
                }
                else
                {
                    MessageBoxResult action = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo()
                    {
                        Message = "PlayList's name existed. Do you want to override it?",
                        Caption = "Save PlayList",
                        Button = MessageBoxButton.YesNo,
                    });
                    if (action == MessageBoxResult.Yes)
                    {
                        File.WriteAllText(filePath, data);
                    }
                }
            }
            catch (DirectoryNotFoundException dnfe)
            {
                System.IO.Directory.CreateDirectory(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(MessageType.Error, "Errors", String.Format("{0}. {1}", ex.Message, ex.InnerException?.Message));
            }
        }
        private List<string> GetAllJsonFile(string FolderName)
        {
            List<string> result = new List<string>();
            try
            {
                DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\" + FolderName + "\\");
                FileInfo[] fi = di.GetFiles("*.json");
                if (fi != null && fi.Length > 0)
                {
                    foreach (var file in fi)
                    {
                        result.Add(file.Name);
                    }
                }
            }
            catch (DirectoryNotFoundException dnfe)
            {
                System.IO.Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"\\{FolderName}\\");
            }
            catch (Exception ex)
            {
                MessageBox.Show(MessageType.Error, "Errors", String.Format("{0}. {1}", ex.Message, ex.InnerException?.Message));
            }
            return result;
        }

        private void PlayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedMedia = (Media)Playlists.SelectedItem;
            if (selectedMedia != null)
            {
                if (selectedMedia.FullPath != null)
                {
                    media.Source = selectedMedia.FullPath;
                    media.Play();
                    isMediaPlaying = true;
                    UpdatePlayButton();
                    CurrentMedia = selectedMedia;
                }
            }
        }

        private void Remove_File_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListBoxItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ListBoxItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void ListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void ListBoxItem_Drop(object sender, DragEventArgs e)
        {

        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Previous_Button_CLick(object sender, RoutedEventArgs e)
        {

        }

        private void Play_Button_CLick(object sender, RoutedEventArgs e)
        {
            if (isMediaPlaying)
            {
                media.Pause();
                isMediaPlaying = false;
            }
            else
            {
                media.Play();
                isMediaPlaying = true;
            }
            UpdatePlayButton();
        }

        private void Next_Button_CLick(object sender, RoutedEventArgs e)
        {
            var currentMediaIndex = Playlists.SelectedIndex;
            int mediaIndex = -1;

            if (isMediaSuffle)
            {
                Random randInt = new Random();
                mediaIndex = randInt.Next(0, _PlayLists.Count);
                while (currentMediaIndex == mediaIndex)
                {
                    mediaIndex = randInt.Next(0, _PlayLists.Count);
                }
            }
            else
            {
                mediaIndex = currentMediaIndex++;
            }
            Playlists.SelectedIndex = mediaIndex;
        }

        private void Shuffle_Button_CLick(object sender, RoutedEventArgs e)
        {
            if (isMediaSuffle)
            {
                isMediaSuffle = false;
                IsSuffle.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.ShuffleDisabled;
            }
            else
            {
                IsSuffle.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Shuffle;
                isMediaSuffle = true;
            }
            UpdatePlayButton();
        }
        private void UpdatePlayButton()
        {
            if (isMediaPlaying)
            {
                IsPlaying.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
            }
            else
            {
                IsPlaying.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
            }
        }

        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            var currentMediaIndex = Playlists.SelectedIndex;
            int mediaIndex = -1;

            if (isMediaSuffle)
            {
                Random randInt = new Random();
                mediaIndex = randInt.Next(0, _PlayLists.Count - 1);
                while (currentMediaIndex == mediaIndex)
                {
                    mediaIndex = randInt.Next(0, _PlayLists.Count - 1);
                }
            }
            else
            {
                mediaIndex = currentMediaIndex++;
            }
            Playlists.SelectedIndex = mediaIndex;
        }

    }
}

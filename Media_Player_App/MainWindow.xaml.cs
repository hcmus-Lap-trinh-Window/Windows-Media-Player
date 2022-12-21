using HandyControl.Data;
using HandyControl.Tools.Extension;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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
        private bool userIsDraggingTimeSlider = false;
        private bool userIsDraggingVolumeSlider = false;

        private DispatcherTimer timer;
        private List<int> _PlaylistHistory = new List<int>();
        private List<Media> _RecentlyPlayed = new List<Media>();

        private Media CurrentMedia = null;
        private ObservableCollection<Media> _PlayLists;
        private ObservableCollection<String> _PlayListComboBox;

        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((media.Source != null) && (media.NaturalDuration.HasTimeSpan) && (!userIsDraggingTimeSlider))
            {
                slider.Minimum = 0;
                slider.Maximum = media.NaturalDuration.TimeSpan.TotalSeconds;
                slider.Value = media.Position.TotalSeconds;

                currentPosition.Text = TimeSpan.FromSeconds(slider.Value).ToString(@"hh\:mm\:ss");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // init flag for tracking media player's state
            isMediaPlaying = false;
            isMediaSuffle = true;

            if (isMediaSuffle)
            {
                IsSuffle.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Shuffle;
                shuffeMode.Text = "Shuffle: ";
            }
            else
            {
                IsSuffle.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.ShuffleDisabled;
                shuffeMode.Text = "Normal: ";
            }

            // init playlist
            _PlayLists = new ObservableCollection<Media>();
            Playlists.ItemsSource = _PlayLists;
            Playlists.SelectionMode = System.Windows.Controls.SelectionMode.Single;

            // init playlist combo box
            _PlayListComboBox = new ObservableCollection<string>(GetAllJsonFile(Utilities.PlayListFolder).Select(c => c.getFileName()).OrderBy(c => c).ToList());
            PlaylistComboBox.ItemsSource = _PlayListComboBox;

            // load recently played
            var recentlyPlayedFileName = GetAllJsonFile(Utilities.RecentlyPlayed).LastOrDefault();
            if (!string.IsNullOrEmpty(recentlyPlayedFileName))
            {
                var recentlyPlayedDirectory = Directory.GetCurrentDirectory() + $@"\\{Utilities.RecentlyPlayed}\\{recentlyPlayedFileName}";
                var recentlyPlayedJson = File.ReadAllText(recentlyPlayedDirectory);
                if (recentlyPlayedJson != null)
                {
                    _RecentlyPlayed = JsonSerializer.Deserialize<List<Media>>(recentlyPlayedJson);
                }
            }
        }

        private void New_File_Button_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            screen.Filter = "Media Files (*.mp3; *.mp4)|*.mp3;*.mp4|All files (*.*)|*.*";
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
                    handleChangeWhenAddNewFile(_currentPlaying, newMedia);
                }
            }
        }

        private void handleChangeWhenAddNewFile(string filePath, Media newMedia)
        {
            // add new media to playlist and playlist history
            _PlayLists.Add(newMedia);
            if (isMediaSuffle && _PlayLists.Count > 1)
            {
                _PlaylistHistory.Add(_PlayLists.IndexOf(newMedia) - 1);
            }

            #region set change in UI

            media.Source = new Uri(filePath, UriKind.Absolute);
            media.Play();

            Next_Button.IsEnabled = _PlayLists.Count > 1 ? true : false;
            Previous_Button.IsEnabled = _PlayLists.Count > 1 ? true : false;

            CurrentMedia = newMedia;
            Playlists.SelectedItem = CurrentMedia;

            _RecentlyPlayed.Add(newMedia);
            SaveRecentlyPlayed();

            #endregion                    
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
                var playListDirectory = Directory.GetCurrentDirectory() + $"\\{Utilities.PlayListFolder}\\{playListSelected}.json";
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
                        var playListDirectory = Directory.GetCurrentDirectory() + $@"\\{Utilities.PlayListFolder}\\";
                        var playListJson = JsonSerializer.Serialize(_PlayLists);
                        SaveJson(playListDirectory + playListFileName, playListJson);
                        if (!_PlayListComboBox.Contains(playListName))
                        {
                            _PlayListComboBox.Add(playListName);
                            HandyControl.Controls.MessageBox.Show(new MessageBoxInfo()
                            {
                                Message = "Save PlayList Successfully",
                                Caption = "Save PlayList",
                                Button = MessageBoxButton.OK,
                            });
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
            playPreviousMediaFile();
        }

        private void playPreviousMediaFile()
        {
            var currentMediaIndex = Playlists.SelectedIndex;
            int mediaIndex;

            if (isMediaSuffle)
            {
                if (_PlaylistHistory.Count > 0)
                {
                    var indexToBack = _PlaylistHistory.Count - 1;
                    mediaIndex = _PlaylistHistory[indexToBack];
                    _PlaylistHistory.RemoveAt(indexToBack);
                }
                else
                {
                    Previous_Button.IsEnabled = false;
                    return;
                }
            }
            else
            {
                mediaIndex = --currentMediaIndex;

                if (mediaIndex < 0)
                {
                    Previous_Button.IsEnabled = false;
                    return;
                }
            }
            Playlists.SelectedIndex = mediaIndex;
        }

        private void Play_Button_CLick(object sender, RoutedEventArgs e)
        {
            playOrPauseCurrentMediaFile();
        }

        private void playOrPauseCurrentMediaFile()
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
            playNextMediaFile();
        }

        private void playNextMediaFile()
        {
            var currentMediaIndex = Playlists.SelectedIndex;
            int mediaIndex;
            Previous_Button.IsEnabled = true;

            if (isMediaSuffle)
            {
                Random randInt = new Random();
                mediaIndex = randInt.Next(0, _PlayLists.Count);
                while (currentMediaIndex == mediaIndex)
                {
                    mediaIndex = randInt.Next(0, _PlayLists.Count);
                }

                // add to history for previous button click
                _PlaylistHistory.Add(currentMediaIndex);
            }
            else
            {
                mediaIndex = ++currentMediaIndex;

                if (mediaIndex >= _PlayLists.Count)
                    mediaIndex = 0;
            }
            Playlists.SelectedIndex = mediaIndex;
        }

        private void Shuffle_Button_CLick(object sender, RoutedEventArgs e)
        {
            UpdateShuffleButton();
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
        private void UpdateShuffleButton()
        {
            if (isMediaSuffle)
            {
                if (isMediaSuffle)
                {
                    isMediaSuffle = false;
                    IsSuffle.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.ShuffleDisabled;
                    shuffeMode.Text = "Normal:";
                }
                else
                {
                    IsSuffle.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Shuffle;
                    isMediaSuffle = true;
                    shuffeMode.Text = "Shuffle:";
                }
                UpdatePlayButton();
            }
        }

        private void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            #region set change in UI
            Control_Button_Group.Visibility = Visibility.Visible;
            Progress_Time.Visibility = Visibility.Visible;
            Shuffle_Volume_Group.Visibility = Visibility.Visible;

            Volume_Progress.Value = media.Volume;
            setVolumeKind(media.Volume);
            totalPosition.Text = TimeSpan.FromSeconds(media.NaturalDuration.TimeSpan.TotalSeconds).ToString(@"hh\:mm\:ss");

            isMediaPlaying = true;
            UpdatePlayButton();

            name.Text = CurrentMedia.Name;
            singer.Text = CurrentMedia.Singer;            
            audioImagePath.ImageSource = Utilities.covertStringtoBitmapImage(CurrentMedia.ImagePath);
            audioImagePath_border.BorderBrush = Brushes.White;
            #endregion

        }

        private void setVolumeKind(double volume)
        {
            if(volume == 0 || media.IsMuted)
                Volume.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.VolumeOff;
            else if(volume > 0 && volume < 0.3)
                Volume.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.VolumeLow;
            else if (volume >= 0.3 && volume <= 0.7)
                Volume.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.VolumeMedium;
            else
                Volume.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.VolumeHigh;
        }

        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            var currentMediaIndex = Playlists.SelectedIndex;
            int mediaIndex;

            if(_PlayLists.Count == 1)
            {
                media.Pause();
                isMediaPlaying = false;
                UpdatePlayButton();
                
                return;
            }

            if (isMediaSuffle)
            {
                Random randInt = new Random();
                mediaIndex = randInt.Next(0, _PlayLists.Count);
                while (currentMediaIndex == mediaIndex)
                {
                    mediaIndex = randInt.Next(0, _PlayLists.Count);
                }

                // add to history for previous button click
                _PlaylistHistory.Add(currentMediaIndex);
            }
            else
            {
                mediaIndex = ++currentMediaIndex;

                if (mediaIndex >= _PlayLists.Count)
                    mediaIndex = 0;
            }
            Playlists.SelectedIndex = mediaIndex;
        }

        private void slideProgress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            userIsDraggingTimeSlider = true;
        }

        private void slideProgress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            userIsDraggingTimeSlider = false;
            media.Position = TimeSpan.FromSeconds(slider.Value);
        }

        private void slideProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            currentPosition.Text = TimeSpan.FromSeconds(slider.Value).ToString(@"hh\:mm\:ss");
        }

        private void Volume_Button_CLick(object sender, RoutedEventArgs e)
        {
            media.IsMuted = !media.IsMuted;
            setVolumeKind(media.Volume);            
        }

        private void VolumeProgress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            userIsDraggingVolumeSlider = true;
        }

        private void VolumeProgress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            userIsDraggingVolumeSlider = false;
            media.Volume = Volume_Progress.Value;
            setVolumeKind(media.Volume);
        }

        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)                       // Next media
            {
                playNextMediaFile();
            }
            else if (e.Key == Key.P && Keyboard.Modifiers == ModifierKeys.Control)                  // Previous media
            {
                playPreviousMediaFile();
            }
            else if (e.Key == Key.K && Keyboard.Modifiers == ModifierKeys.Control)                  // Play/Pause media
            {
                playOrPauseCurrentMediaFile();
            }
            else if (e.Key == Key.M && Keyboard.Modifiers == ModifierKeys.Control)                  // Mute/Unmute media
            {
                media.IsMuted = !media.IsMuted;
            }
            else if (e.Key == Key.J && Keyboard.Modifiers == ModifierKeys.Control)                  // Move back 10 seconds
            {
                media.Position -= media.Position >= TimeSpan.FromSeconds(10) ? TimeSpan.FromSeconds(10) : media.Position;
                slider.Value = media.Position.TotalSeconds;
                currentPosition.Text = TimeSpan.FromSeconds(slider.Value).ToString(@"hh\:mm\:ss");
            }
            else if (e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Control)                  // Move forward 10 seconds
            {
                var remainTime = TimeSpan.FromSeconds(media.NaturalDuration.TimeSpan.TotalSeconds) - media.Position;

                media.Position += remainTime >= TimeSpan.FromSeconds(10) ? TimeSpan.FromSeconds(10) : remainTime;
                slider.Value = media.Position.TotalSeconds;
                currentPosition.Text = TimeSpan.FromSeconds(slider.Value).ToString(@"hh\:mm\:ss");
            }
            else if (e.Key == Key.Home)                                                             // Move to beginning
            {
                media.Position = TimeSpan.FromSeconds(0);
                slider.Value = 0;
                currentPosition.Text = TimeSpan.FromSeconds(slider.Value).ToString(@"hh\:mm\:ss");
            }
            else if (e.Key == Key.End)                                                              // Move to end
            {
                media.Position = TimeSpan.FromSeconds(media.NaturalDuration.TimeSpan.TotalSeconds);
                slider.Value = media.Position.TotalSeconds;
                currentPosition.Text = TimeSpan.FromSeconds(slider.Value).ToString(@"hh\:mm\:ss");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveRecentlyPlayed();
        }

        public void SaveRecentlyPlayed()
        {
            try
            {
                var recentlyPlayedJson = JsonSerializer.Serialize(_RecentlyPlayed);
                if (recentlyPlayedJson != null )
                {
                    var fileName = Directory.GetCurrentDirectory() + @$"\\{Utilities.RecentlyPlayed}\\RecentlyPlayed.json";
                    SaveJson(fileName, recentlyPlayedJson, true);
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException ?? ex);
            }
        }
    }
}

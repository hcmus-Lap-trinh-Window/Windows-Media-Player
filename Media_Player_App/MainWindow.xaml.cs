using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
        private bool isPlaying = false;
        public MainWindow()
        {
            InitializeComponent();
            isPlaying= true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void New_File_Button_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                var _currentPlaying = screen.FileName;
                //this.Title = $"Opened: {_shortName}";
                media.Source = new Uri(_currentPlaying, UriKind.Absolute);
                media.Play();
                //media.Stop();

                //_timer = new DispatcherTimer();
                //_timer.Interval = new TimeSpan(0, 0, 0, 1, 0); ;
                //_timer.Tick += _timer_Tick;
            }
        }

        private void PlaylistComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Clear_All_Files_In_Playlist_Btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Add_Playlist_Button_CLick(object sender, RoutedEventArgs e)
        {

        }

        private void PlayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
            try
            {
                if (media.Source != null)
                {
                    if (isPlaying)
                    {
                        media.Pause();
                        isPlaying = false;
                        IsPlaying.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
                    }
                    else
                    {
                        media.Play();
                        isPlaying = true;
                        IsPlaying.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
                    }
                }
            }
            catch(Exception ex) {
                throw new Exception(ex.Message, ex.InnerException?? ex);
            }
        }

        private void Next_Button_CLick(object sender, RoutedEventArgs e)
        {

        }

        private void Shuffle_Button_CLick(object sender, RoutedEventArgs e)
        {

        }
    }
}

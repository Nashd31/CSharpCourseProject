using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Telhai.DotNet.PlayerProject.Models;
using Telhai.DotNet.PlayerProject.Services;

namespace Telhai.DotNet.PlayerProject
{
    public partial class MusicPlayer : Window
    {
        private readonly MediaPlayer mediaPlayer = new();
        private readonly DispatcherTimer progressTimer = new();
        private readonly DispatcherTimer slideshowTimer = new();

        private List<MusicTrack> library = [];
        private readonly ItunesService itunesService = new();
        private CancellationTokenSource? cts;

        private const string FILE_NAME = "library.json";
        private readonly List<string> currentGallery = [];
        private int currentImageIndex = 0;

        public MusicPlayer()
        {
            InitializeComponent();
            SetupTimers();
            LoadLibrary();
        }

        private void SetupTimers()
        {
            progressTimer.Interval = TimeSpan.FromMilliseconds(100);
            progressTimer.Tick += (s, e) =>
            {
                if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    sliderProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                    sliderProgress.Value = mediaPlayer.Position.TotalSeconds;
                    lblCurrentTime.Text = mediaPlayer.Position.ToString(@"mm\:ss");
                    lblTotalTime.Text = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                }
            };

            slideshowTimer.Interval = TimeSpan.FromSeconds(3);
            slideshowTimer.Tick += SlideshowTimer_Tick;
        }

        // Single click displays Name and Path
        private async void LstLibrary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack selectedTrack)
            {
                // Update labels
                txtCurrentSong.Text = selectedTrack.Title;
                txtFilePath.Text = selectedTrack.FilePath;

                // Load metadata (API or JSON) in the background
                await LoadTrackMetadata(selectedTrack, false);
            }
        }

        // Double click plays the song
        private async void LstLibrary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                mediaPlayer.Open(new Uri(track.FilePath, UriKind.Absolute));
                mediaPlayer.Play();
                progressTimer.Start();

                await LoadTrackMetadata(track, true);
            }
        }

        private async Task LoadTrackMetadata(MusicTrack track, bool isPlaying)
        {
            // Cancel previous API calls
            cts?.Cancel();
            cts = new CancellationTokenSource();

            // Check JSON first, don't call API if metadata exists
            if (!track.HasMetadata)
            {
                ItunesTrackInfo? info = await itunesService.SearchOneAsync(track.Title, cts.Token);
                if (info != null)
                {
                    track.ArtistName = info.ArtistName;
                    track.AlbumName = info.AlbumName;
                    track.ArtworkUrl = info.ArtworkUrl;
                    track.HasMetadata = true;
                    SaveLibrary(); // Save new API data to JSON
                }
            }

            UpdateUI(track);
            if (isPlaying)
            {
                SetupGalleryAndSlideshow(track);
            }
        }

        private void UpdateUI(MusicTrack track)
        {
            txtCurrentSong.Text = track.Title;
            txtArtistAlbum.Text = $"{track.ArtistName ?? "Unknown Artist"} - {track.AlbumName ?? "Unknown Album"}";

            // Default image logic
            if (!string.IsNullOrEmpty(track.ArtworkUrl))
            {
                SetArtwork(track.ArtworkUrl);
            }
            else
            {
                SetArtwork("pack://application:,,,/Pictures/default_Video.jpg");
            }
        }

        private void SetupGalleryAndSlideshow(MusicTrack track)
        {
            currentGallery.Clear();

            // Use User images if they exist, otherwise use API image
            if (track.UserImages != null && track.UserImages.Count > 0)
            {
                currentGallery.AddRange(track.UserImages);
            }
            else if (!string.IsNullOrEmpty(track.ArtworkUrl))
            {
                currentGallery.Add(track.ArtworkUrl);
            }

            if (currentGallery.Count > 1)
            {
                currentImageIndex = 0;
                slideshowTimer.Start();
            }
            else
            {
                slideshowTimer.Stop();
                if (currentGallery.Count == 1)
                {
                    SetArtwork(currentGallery[0]);
                }
            }
        }

        private void SlideshowTimer_Tick(object? sender, EventArgs e)
        {
            if (currentGallery.Count == 0)
            {
                return;
            }

            currentImageIndex = (currentImageIndex + 1) % currentGallery.Count;
            SetArtwork(currentGallery[currentImageIndex]);
        }

        private void SetArtwork(string path)
        {
            try { imgAlbumArt.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute)); }
            catch { /* Fallback to default if path is broken */ }
        }

        // Persistence Logic
        private void SaveLibrary()
        {
            File.WriteAllText(FILE_NAME, JsonSerializer.Serialize(library));
        }

        private void LoadLibrary()
        {
            if (File.Exists(FILE_NAME))
            {
                library = JsonSerializer.Deserialize<List<MusicTrack>>(File.ReadAllText(FILE_NAME)) ?? [];
                lstLibrary.ItemsSource = library;
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                EditTrackWindow editWin = new(track);
                if (editWin.ShowDialog() == true)
                {
                    SaveLibrary(); // Save changes to JSON
                    UpdateUI(track);
                }
            }
        }

        // Basic Player Commands
        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                if (mediaPlayer.Source == null || !mediaPlayer.Source.LocalPath.Equals(track.FilePath, StringComparison.OrdinalIgnoreCase))
                {
                    mediaPlayer.Open(new Uri(track.FilePath, UriKind.Absolute));
                }

                mediaPlayer.Play();
                progressTimer.Start();
                SetupGalleryAndSlideshow(track);
            }
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            slideshowTimer.Stop();
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = sliderVolume.Value;
        }

        private void Slider_DragStarted(object sender, MouseButtonEventArgs e)
        {
            progressTimer.Stop();
        }

        private void Slider_DragCompleted(object sender, MouseButtonEventArgs e)
        {
            mediaPlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
            progressTimer.Start();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new() { Filter = "MP3 files (*.mp3)|*.mp3" };
            if (dlg.ShowDialog() == true)
            {
                MusicTrack newTrack = new()
                {
                    Title = Path.GetFileNameWithoutExtension(dlg.FileName),
                    FilePath = dlg.FileName
                };
                library.Add(newTrack);
                lstLibrary.ItemsSource = null; 
                lstLibrary.ItemsSource = library;
                SaveLibrary();
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                library.Remove(track);
                lstLibrary.ItemsSource = null; 
                lstLibrary.ItemsSource = library;
                SaveLibrary();
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            // Create the settings window
            SettingsWindow settingsWin = new();

            // Subscribe to the OnScanCompleted event
            settingsWin.OnScanCompleted += (newTracks) =>
            {
                bool libraryUpdated = false;

                foreach (MusicTrack track in newTracks)
                {
                    // Check if the song already exists in the library by its file path
                    if (!library.Any(x => x.FilePath == track.FilePath))
                    {
                        library.Add(track);
                        libraryUpdated = true;
                    }
                }

                if (libraryUpdated)
                {
                    // Refresh the UI list
                    lstLibrary.ItemsSource = null;
                    lstLibrary.ItemsSource = library;

                    // Save the updated library to the JSON file
                    SaveLibrary();
                }
            };

            // Show the window
            settingsWin.ShowDialog();
        }
    }
}
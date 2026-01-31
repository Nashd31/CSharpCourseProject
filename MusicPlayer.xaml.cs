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
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private DispatcherTimer timer = new DispatcherTimer();
        private List<MusicTrack> library = new List<MusicTrack>();
        private readonly ItunesService itunesService = new ItunesService();
        private CancellationTokenSource? cts; // Used to cancel pending API requests when switching songs
        private const string FILE_NAME = "library.json";

        public MusicPlayer()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            this.Loaded += MusicPlayer_Loaded;
        }

        private void MusicPlayer_Loaded(object sender, RoutedEventArgs e) => LoadLibrary();

        // Load saved songs from JSON file on startup
        private void LoadLibrary()
        {
            if (File.Exists(FILE_NAME))
            {
                string json = File.ReadAllText(FILE_NAME);
                library = JsonSerializer.Deserialize<List<MusicTrack>>(json) ?? new List<MusicTrack>();
                UpdateLibraryUI();
            }
        }

        private void SaveLibrary()
        {
            string json = JsonSerializer.Serialize(library);
            File.WriteAllText(FILE_NAME, json);
        }

        private void UpdateLibraryUI()
        {
            lstLibrary.ItemsSource = null;
            lstLibrary.ItemsSource = library;
        }

        // Single click: Show information without starting playback
        private void LstLibrary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                txtCurrentSong.Text = track.Title;
                txtFilePath.Text = track.FilePath;

                if (!string.IsNullOrEmpty(track.ArtistName))
                {
                    txtArtistAlbum.Text = $"{track.ArtistName} - {track.AlbumName}";
                    if (!string.IsNullOrEmpty(track.ArtworkUrl))
                        imgAlbumArt.Source = new BitmapImage(new Uri(track.ArtworkUrl));
                }
                else
                {
                    ClearMetadataDisplay(); // Reset UI if no data is found yet
                }
            }
        }

        private async void LstLibrary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                await PlayAndFetchAsync(track);
            }
        }

        // Handles local playback and external API call in parallel
        private async Task PlayAndFetchAsync(MusicTrack track)
        {
            if (File.Exists(track.FilePath))
            {
                mediaPlayer.Open(new Uri(track.FilePath));
                mediaPlayer.Play();
                timer.Start();
                txtCurrentSong.Text = track.Title;
                txtFilePath.Text = track.FilePath;
            }

            // Cancel the previous API call if it hasn't finished yet
            cts?.Cancel();
            cts = new CancellationTokenSource();

            try
            {
                // Fetch song info from iTunes API asynchronously
                var info = await itunesService.SearchOneAsync(track.Title, cts.Token);
                if (info != null)
                {
                    track.ArtistName = info.ArtistName;
                    track.AlbumName = info.AlbumName;
                    track.ArtworkUrl = info.ArtworkUrl;

                    txtArtistAlbum.Text = $"{info.ArtistName} - {info.AlbumName}";
                    if (!string.IsNullOrEmpty(info.ArtworkUrl))
                        imgAlbumArt.Source = new BitmapImage(new Uri(info.ArtworkUrl));

                    SaveLibrary(); // Persist the fetched metadata
                }
                else
                {
                    txtArtistAlbum.Text = "Metadata not found";
                }
            }
            catch (OperationCanceledException) { /* Request was cancelled by user action */ }
            catch (Exception) { txtArtistAlbum.Text = "Error fetching from API"; }
        }

        // Resets display and loads the local default JPG
        private void ClearMetadataDisplay()
        {
            txtArtistAlbum.Text = "";
            try
            {
                imgAlbumArt.Source = new BitmapImage(new Uri("pack://application:,,,/Pictures/default_Video.jpg"));
            }
            catch { imgAlbumArt.Source = null; }
        }

        private async void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack selectedTrack)
            {
                // If a new song is selected, load and fetch. Otherwise, just resume.
                if (mediaPlayer.Source == null || !mediaPlayer.Source.LocalPath.Equals(selectedTrack.FilePath, StringComparison.OrdinalIgnoreCase))
                {
                    await PlayAndFetchAsync(selectedTrack);
                }
                else
                {
                    mediaPlayer.Play();
                    timer.Start();
                }
            }
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            timer.Stop();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            timer.Stop();
            sliderProgress.Value = 0; // Reset progress bar visual
            lblCurrentTime.Text = "00:00";
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = sliderVolume.Value;
        }

        // Updates the progress bar and time labels
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                TimeSpan current = mediaPlayer.Position;
                TimeSpan total = mediaPlayer.NaturalDuration.TimeSpan;

                sliderProgress.Maximum = total.TotalSeconds;
                sliderProgress.Value = current.TotalSeconds;

                lblCurrentTime.Text = current.ToString(@"mm\:ss");
                lblTotalTime.Text = total.ToString(@"mm\:ss");
            }
        }

        // Pause timer while dragging
        private void Slider_DragStarted(object sender, MouseButtonEventArgs e) 
        {
            timer.Stop();
        }

        private void Slider_DragCompleted(object sender, MouseButtonEventArgs e)
        {
            mediaPlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
            timer.Start();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "MP3 files (*.mp3)|*.mp3" };
            if (openFileDialog.ShowDialog() == true)
            {
                library.Add(new MusicTrack
                {
                    Title = Path.GetFileNameWithoutExtension(openFileDialog.FileName),
                    FilePath = openFileDialog.FileName
                });
                UpdateLibraryUI(); SaveLibrary();
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                library.Remove(track);
                UpdateLibraryUI();
                SaveLibrary();
            }
        }

        // Open settings window and handle the results using an event
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWin = new SettingsWindow();
            settingsWin.OnScanCompleted += (newTracks) =>
            {
                foreach (var track in newTracks)
                    if (!library.Any(x => x.FilePath == track.FilePath)) library.Add(track);

                UpdateLibraryUI(); SaveLibrary();
            };
            settingsWin.ShowDialog();
        }
    }
}
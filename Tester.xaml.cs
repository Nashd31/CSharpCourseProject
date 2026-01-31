
using System.IO;
using System.Windows;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Telhai.DotNet.PlayerProject.Models;
using Telhai.DotNet.PlayerProject.Services;

namespace Telhai.DotNet.PlayerProject
{
    /// <summary>
    /// Interaction logic for Tester.xaml
    /// </summary>
    public partial class Tester : Window
    {

        private readonly ItunesService _itunesService = new ItunesService();
        private CancellationTokenSource? _cts;

        public Tester()
        {
            InitializeComponent();
            this.MouseDoubleClick += Tester_MouseDoubleClick;
        }

        private void Tester_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PlaySong("E:\\NASHED\\Tel-Hai\\5.Semester_5\\4.C#_Language_and_dotNET\\8.Project\\Audio\\kush.mp3");
        }

        private void PlaySong(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            string songName = System.IO.Path.GetFileNameWithoutExtension(filePath);

            
            PlayLocalFile(filePath);

            
            ClearSongInfo();
            StatusText.Text = "Searching song info...";

            
            _ = LoadSongInfoAsync(songName, _cts.Token);
        }

        private void PlayLocalFile(string filePath)
        {
            MediaPlayer player = new MediaPlayer();
            player.Open(new Uri(filePath));
            player.Play();
        }

        private async Task LoadSongInfoAsync(
            string songName,
            CancellationToken token)
        {
            try
            {
                ItunesTrackInfo? info =
                    await _itunesService.SearchOneAsync(songName, token);

                if (info == null)
                {
                    StatusText.Text = "No information found.";
                    return;
                }

                
                Dispatcher.Invoke(() =>
                {
                    TrackNameText.Text = info.TrackName;
                    ArtistNameText.Text = info.ArtistName;
                    AlbumNameText.Text = info.AlbumName;
                    StatusText.Text = "Info loaded.";

                    if (!string.IsNullOrWhiteSpace(info.ArtworkUrl))
                    {
                        AlbumImage.Source =
                            new BitmapImage(new Uri(info.ArtworkUrl));
                    }
                });
            }
            catch (OperationCanceledException)
            {
                // שיר הוחלף – מתעלמים
            }
            catch (Exception)
            {
                Dispatcher.Invoke(() =>
                {
                    StatusText.Text = "Error loading song info.";
                });
            }
        }

        private void ClearSongInfo()
        {
            TrackNameText.Text = "";
            ArtistNameText.Text = "";
            AlbumNameText.Text = "";
            AlbumImage.Source = null;
        }
    }
}
using Microsoft.Win32;
using System.IO;
using System.Windows;
using Telhai.DotNet.PlayerProject.Models;

namespace Telhai.DotNet.PlayerProject
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        private readonly AppSettings currentSettings;

        // Event to send data back to Main Window
        public event Action<List<MusicTrack>>? OnScanCompleted;

        public SettingsWindow()
        {
            InitializeComponent();
            currentSettings = AppSettings.Load();
            RefreshFolderList();
        }

        private void RefreshFolderList()
        {
            lstFolders.ItemsSource = null;
            lstFolders.ItemsSource = currentSettings.MusicFolders;
        }

        // Placeholders to make it build
        private void BtnAddFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new();

            if (dialog.ShowDialog() == true)
            {
                string folder = dialog.FolderName;
                if (!currentSettings.MusicFolders.Contains(folder))
                {
                    currentSettings.MusicFolders.Add(folder);
                    AppSettings.Save(currentSettings);
                    RefreshFolderList();
                }
            }
        }
        private void BtnRemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            if (lstFolders.SelectedItem is string folder)
            {
                _ = currentSettings.MusicFolders.Remove(folder);
                AppSettings.Save(currentSettings);
                RefreshFolderList();
            }
        }
        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            List<MusicTrack> foundTracks = [];

            foreach (string folderPath in currentSettings.MusicFolders)
            {
                if (Directory.Exists(folderPath))
                {
                    // SearchOption.AllDirectories makes it scan sub-folders
                    string[] files = Directory.GetFiles(folderPath, "*.mp3", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        foundTracks.Add(new MusicTrack
                        {
                            Title = Path.GetFileNameWithoutExtension(file),
                            FilePath = file
                        });
                    }
                }
            }

            // Send data back to MainWindow
            OnScanCompleted?.Invoke(foundTracks);

            _ = MessageBox.Show($"Scan Complete! Found {foundTracks.Count} songs.");
            Close();
        }
    }
}

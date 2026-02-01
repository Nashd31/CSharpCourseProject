using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Telhai.DotNet.PlayerProject.Models;

namespace Telhai.DotNet.PlayerProject.ViewModels
{
    public class EditTrackViewModel
    {
        public MusicTrack Track { get; set; }
        public ObservableCollection<string> UserImages { get; set; }

        public ICommand AddImageCommand { get; }
        public ICommand RemoveImageCommand { get; }

        public EditTrackViewModel(MusicTrack track)
        {
            Track = track;
            UserImages = new ObservableCollection<string>(track.UserImages ?? []);

            AddImageCommand = new RelayCommand(AddImage);
            RemoveImageCommand = new RelayCommand<string>(RemoveImage);
        }

        private void AddImage()
        {
            OpenFileDialog dlg = new() { Filter = "Image files (*.jpg;*.png)|*.jpg;*.png" };
            if (dlg.ShowDialog() == true)
            {
                UserImages.Add(dlg.FileName);
            }
        }

        private void RemoveImage(string path)
        {
            if (path != null)
            {
                _ = UserImages.Remove(path);
            }
        }

        // Method to sync back to the model before saving
        public void SyncModel()
        {
            Track.UserImages = [.. UserImages];
            Track.HasMetadata = true;
        }
    }
}
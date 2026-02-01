using System.Windows;
using Telhai.DotNet.PlayerProject.ViewModels;

namespace Telhai.DotNet.PlayerProject
{
    public partial class EditTrackWindow : Window
    {
        public EditTrackWindow(Models.MusicTrack track)
        {
            InitializeComponent();
            DataContext = new EditTrackViewModel(track);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ((EditTrackViewModel)DataContext).SyncModel();
            DialogResult = true;
            Close();
        }
    }
}
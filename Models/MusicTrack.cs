namespace Telhai.DotNet.PlayerProject.Models
{
    public class MusicTrack
    {
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        // Metadata for the API
        public string? ArtistName { get; set; }
        public string? AlbumName { get; set; }
        public string? ArtworkUrl { get; set; }

        // List for user-added images from the Edit Window
        public List<string> UserImages { get; set; } = [];

        // Flag to check if we already have metadata from API/Manual edit
        public bool HasMetadata { get; set; } = false;

        // This makes sure the ListBox shows the Name
        public override string ToString()
        {
            return Title;
        }
    }

}

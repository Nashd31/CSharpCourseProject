using System;
using System.Collections.Generic;
using System.Text;

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

        // This makes sure the ListBox shows the Name
        public override string ToString()
        {
            return Title;
        }
    }

}

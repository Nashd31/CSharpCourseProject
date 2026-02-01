using System.Net.Http;
using System.Text.Json;
using Telhai.DotNet.PlayerProject.Models;



namespace Telhai.DotNet.PlayerProject.Services
{
    public class ItunesService
    {
        private static readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("https://itunes.apple.com/")
        };

        public async Task<ItunesTrackInfo?> SearchOneAsync(
            string songTitle,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(songTitle))
                return null;

            try
            {
                // build the request URL
                string cleanTerm = songTitle.Replace("-", " ").Replace("_", " ");
                string url = $"search?term={Uri.EscapeDataString(cleanTerm)}&media=music&limit=1";

                using HttpResponseMessage response =
                    await _httpClient.GetAsync(url, cancellationToken);

                response.EnsureSuccessStatusCode();

                // Get the response content as a JSON string
                string json = await response.Content.ReadAsStringAsync(cancellationToken);

                // Deserialize the Json object 
                ItunesSearchResponse? data = JsonSerializer.Deserialize<ItunesSearchResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                ItunesResultItem? item = data?.Results?.FirstOrDefault();
                return item == null
                    ? null
                    : new ItunesTrackInfo
                    {
                        TrackName = item.TrackName,
                        ArtistName = item.ArtistName,
                        AlbumName = item.CollectionName,
                        ArtworkUrl = item.ArtworkUrl100
                    };
            }
            catch (OperationCanceledException) { return null; }
            catch { return null; }
        }
    }
}


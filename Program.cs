using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

class Program
{
    const string apiKeyGif = "voaNIOg1u7ONPbckzWK71C48YqCOkhVP";

    static async Task Main(string[] args)
    {
        using HttpClient httpClient = new();

        try
        {
            // Paso 1: Obtener fact
            string catFactUrl = "https://catfact.ninja/fact";
            var catResponse = await httpClient.GetAsync(catFactUrl);
            catResponse.EnsureSuccessStatusCode();

            var catJson = await catResponse.Content.ReadAsStringAsync();
            var catData = JsonSerializer.Deserialize<CatFact>(catJson);

            Console.WriteLine($"Fast Cat fact is: {catData!.Fact}");

            // Paso 2: Tomar 3 palabras
            string[] words = catData.Fact.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string search = string.Join(' ', words[..Math.Min(3, words.Length)]);
            Console.WriteLine($"Search query: {search}");

            // Paso 3: Buscar en Giphy
            string gifUrl = $"https://api.giphy.com/v1/gifs/search?api_key={apiKeyGif}&q={Uri.EscapeDataString(search)}&limit=1";
            var gifResponse = await httpClient.GetAsync(gifUrl);
            gifResponse.EnsureSuccessStatusCode();

            var gifJson = await gifResponse.Content.ReadAsStringAsync();
            Console.WriteLine("Giphy JSON: " + gifJson);

            var gifData = JsonSerializer.Deserialize<GifResponse>(gifJson);

            if (gifData == null || gifData.Data == null || gifData.Data.Length == 0)
            {
                Console.WriteLine("No gif found.");
                return;
            }

            string gifFinalUrl = gifData.Data[0].Images.Original.Url;
            Console.WriteLine($"Shown Gif: {gifFinalUrl}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }

    public class CatFact
    {
        [JsonPropertyName("fact")]
        public string Fact { get; set; } = default!;

        [JsonPropertyName("length")]
        public int Length { get; set; }
    }

    public class GifResponse
    {
        public GifData[] Data { get; set; } = default!;
    }

    public class GifData
    {
        public GifImages Images { get; set; } = default!;
    }

    public class GifImages
    {
        [JsonPropertyName("original")]
        public OriginalGif Original { get; set; } = default!;
    }

    public class OriginalGif
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = default!;
    }
}

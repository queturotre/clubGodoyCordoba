using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

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

            string fact = catData!.Fact;
            Console.WriteLine($"Fast Cat fact is: {fact}");

            // Paso 2: Tomar 3 palabras
            string[] words = fact.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string queryWords = string.Join(' ', words[..Math.Min(3, words.Length)]);
            Console.WriteLine($"Search query: {queryWords}");

            // Paso 3: Buscar en Giphy
            string gifUrl = $"https://api.giphy.com/v1/gifs/search?api_key={apiKeyGif}&q={Uri.EscapeDataString(queryWords)}&limit=1";
            var gifResponse = await httpClient.GetAsync(gifUrl);
            gifResponse.EnsureSuccessStatusCode();

            var gifJson = await gifResponse.Content.ReadAsStringAsync();

            var gifData = JsonSerializer.Deserialize<GifResponse>(gifJson);

            string gifFinalUrl;

            if (gifData == null || gifData.Data == null || gifData.Data.Length == 0)
            {
                Console.WriteLine("No gif found.");
                gifFinalUrl = "No Url Found";
            }
            else
            {
                gifFinalUrl = gifData.Data[0].Images.Original.Url;
                Console.WriteLine($"Shown Gif: {gifFinalUrl}");
            }

            // Paso 4: Insertar en MySQL
            string connectionString = "server=localhost;user=app_user;password=$illyGr1p86;database=clubGodoyCordoba";

            Console.WriteLine("Accessing the database");
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    Console.WriteLine("Connection opened");
                    MySqlCommand cmd = new MySqlCommand("CALL Usp_InsertData(@fact, @queryWords, @url);", conn);
                    cmd.Parameters.AddWithValue("@fact", fact);
                    cmd.Parameters.AddWithValue("@queryWords", queryWords);
                    cmd.Parameters.AddWithValue("@url", gifFinalUrl);

                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Success inserting into the database.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error when inserting in the database: {ex.Message}");
                }
            }
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
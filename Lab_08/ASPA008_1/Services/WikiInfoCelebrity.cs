using System.Text.Json;

namespace ASPA008_1.Services;

public sealed class WikiInfoCelebrity
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<WikiInfoCelebrity> logger;

    public WikiInfoCelebrity(IHttpClientFactory httpClientFactory, ILogger<WikiInfoCelebrity> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetReferencesAsync(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return new Dictionary<string, string>();
        }

        try
        {
            HttpClient client = httpClientFactory.CreateClient("Wikipedia");
            string query = "w/api.php?action=opensearch" +
                $"&search={Uri.EscapeDataString(fullName)}" +
                "&limit=5&namespace=0&format=json";

            using HttpResponseMessage response = await client.GetAsync(query);
            if (!response.IsSuccessStatusCode)
            {
                return new Dictionary<string, string>();
            }

            await using Stream stream = await response.Content.ReadAsStreamAsync();
            using JsonDocument document = await JsonDocument.ParseAsync(stream);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 4)
            {
                return new Dictionary<string, string>();
            }

            JsonElement titles = root[1];
            JsonElement urls = root[3];
            Dictionary<string, string> references = new(StringComparer.OrdinalIgnoreCase);
            int count = Math.Min(titles.GetArrayLength(), urls.GetArrayLength());

            for (int i = 0; i < count; i++)
            {
                string? title = titles[i].GetString();
                string? url = urls[i].GetString();
                if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(url))
                {
                    references[title] = url;
                }
            }

            return references;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Could not load Wikipedia references for {FullName}.", fullName);
            return new Dictionary<string, string>();
        }
    }
}

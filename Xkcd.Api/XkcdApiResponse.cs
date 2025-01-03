using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xkcd.Api;

public class XkcdApiResponse
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    
    public string Alt { get; init; }

    [JsonPropertyName("num")]
    public int ComicId { get; init; }

    [JsonPropertyName("img")]
    public string ImageUrl { get; init; }

    //use safe title to avoid HTML like in comic #472 
    [JsonPropertyName("safe_title")]
    public string Title { get; init; }

    public string Month { get; init; }
    public string Day { get; init; }
    public string Year { get; init; }

    [JsonIgnore]
    public string Date => $"{Year}-{Pad(Month)}-{Pad(Day)}";

    private string Pad(string s)
        => s.Length == 1 ? "0" + s : s;
        
    public static XkcdApiResponse FromJson(string json)
        => JsonSerializer.Deserialize<XkcdApiResponse>(json, JsonSerializerOptions);
}
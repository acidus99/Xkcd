using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xkcd.Api;

public class XkcdApiResponse
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    
    public required string Alt { get; init; }

    [JsonPropertyName("num")]
    public required int ComicId { get; init; }

    [JsonPropertyName("img")]
    public required string ImageUrl { get; init; }

    //use safe title to avoid HTML like in comic #472 
    [JsonPropertyName("safe_title")]
    public required string Title { get; init; }

    public required string Month { get; init; }
    public required string Day { get; init; }
    public required string Year { get; init; }

    [JsonIgnore]
    public string Date => $"{Year}-{Pad(Month)}-{Pad(Day)}";

    private string Pad(string s)
        => s.Length == 1 ? "0" + s : s;

    public static XkcdApiResponse FromJson(string json)
    {
        var ret = JsonSerializer.Deserialize<XkcdApiResponse>(json, JsonSerializerOptions);
        if (ret == null)
        {
            throw new ArgumentException("Invalid xkcd json", nameof(json));
        }

        return ret;
    }
}
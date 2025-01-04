namespace Xkcd.Api;

public class XkcdClient : IDisposable
{
    private readonly HttpClient _client = new();

    public int GetLatestComicId()
    {
        var json = GetJson("https://xkcd.com/info.0.json");
        XkcdApiResponse parsedComic = XkcdApiResponse.FromJson(json);
        return parsedComic.ComicId;
    }

    public string GetComicJson(int comicId)
        => GetJson($"https://xkcd.com/{comicId}/info.0.json");

    public byte[] GetComicImage(string url)
        => _client.GetByteArrayAsync(url).GetAwaiter().GetResult();

    private string GetJson(string url)
    {
        return _client.GetStringAsync(url).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}


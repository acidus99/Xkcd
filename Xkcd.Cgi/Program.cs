using System.Text.RegularExpressions;
using Gemini.Cgi;
using Xkcd.Api;

namespace Xkcd.Cgi;

public class Program
{
    private const string ArchivePath = "public_root/xkcd/archive/";
    private static readonly Regex AllDigits = new(@"^\d+$", RegexOptions.Compiled);

    public static void Main(string[] args)
    {

        CgiWrapper cgi = new();
        string query = cgi.SantiziedQuery;
        
        if (query == "latest")
        {
            ShowLatestComic(cgi);
        }
        else if (AllDigits.IsMatch(query))
        {
            var comicId = Convert.ToInt32(query);
            ShowSpecificComic(cgi, comicId);
        }
        else
        {
            ShowRandomComic(cgi);
        }
    }

    private static void ShowLatestComic(CgiWrapper cgi)
    {
        var ids = GetComicIds();
        if (ids.Count == 0)
        {
            cgi.Failure("No comics found");
            return;
        }
        ShowSpecificComic(cgi, ids.Max());
    }

    private static void ShowRandomComic(CgiWrapper cgi)
    {
        var ids = GetComicIds();
        if (ids.Count == 0)
        {
            cgi.Failure("No comics found");
            return;
        }
        Random rand = new Random();
        int id = rand.Next(ids.Count);
        
        ShowSpecificComic(cgi, id);
    }
    
    private static void ShowSpecificComic(CgiWrapper cgi, int comicId)
    {
        cgi.Success("image/png");
        //var metadata = XkcdApiResponse.FromJson($"{ArchivePath}{comicId}.json");
        
        var filename = $"{ArchivePath}{comicId}.png";
        cgi.Out.Write(File.ReadAllBytes(filename));
    }

    static List<int> GetComicIds()
        => Directory.GetFiles(ArchivePath, "*.png")
            .Select(x => Path.GetFileNameWithoutExtension(x))
            .Where(x => AllDigits.IsMatch(x))
            .Select(x => Convert.ToInt32(x)).ToList();
}
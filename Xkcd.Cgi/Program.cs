using System.Text.RegularExpressions;
using Gemini.Cgi;
using Xkcd.Api;

namespace Xkcd.Cgi;

public static class Program
{
    private const string ArchivePath = "public_root/xkcd/archive/";
    private static readonly Regex AllDigits = new(@"^\d+$", RegexOptions.Compiled);

    public static void Main(string[] args)
    {
        using (CgiWrapper cgi = new())
        {
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
        if (!File.Exists(GetJsonPath(comicId)))
        {
            cgi.Failure("Unknown comic ID");
            return;
        }
        
        bool nextExists = File.Exists(GetJsonPath(comicId+1));
        bool prevExists = File.Exists(GetJsonPath(comicId - 1));
        
        var metadata = XkcdApiResponse.FromJson(File.ReadAllText(GetJsonPath(comicId)));
        cgi.Success();
        cgi.Writer.WriteLine($"# XKCD #{comicId} ");
        cgi.Writer.WriteLine($"## {metadata.Title}");
        cgi.Writer.WriteLine(metadata.Date);
        cgi.Writer.WriteLine($"=> /xkcd/archive/{comicId}.png Comic Image: {metadata.Title}");
        metadata.Alt.Split('\n').ToList().ForEach(x => cgi.Writer.WriteLine("> "+ x));
        cgi.Writer.WriteLine("");
        if (nextExists)
        {
            cgi.Writer.WriteLine($"=> ?{comicId+1} ⏭️ Next Comic");
        }
        if (prevExists)
        {
            cgi.Writer.WriteLine($"=> ?{comicId-1} ⏮️ Previous Comic");
        }
        cgi.Writer.WriteLine("=> /xkcd/archive/ View List");
        cgi.Writer.WriteLine("=> /cgi-bin/xkcd.cgi 🎲 Random Comic");
    }

    private static string GetJsonPath(int comicId)
        => $"{ArchivePath}{comicId}.json";

    static List<int> GetComicIds()
        => Directory.GetFiles(ArchivePath, "*.png")
            .Select(x => Path.GetFileNameWithoutExtension(x))
            .Where(x => AllDigits.IsMatch(x))
            .Select(x => Convert.ToInt32(x)).ToList();
}
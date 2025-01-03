using System.Text.RegularExpressions;

namespace Xkcd.Cgi;

public class Program
{
    private const string ArchivePath = "public_root/xkcd/archive/";
    private static readonly Regex AllDigits = new(@"^\d+$", RegexOptions.Compiled);

    public static void Main(string[] args)
    {
        Console.Write("20 image/png\r\n");

        var filename = GetImageForRequest();
        Console.OpenStandardOutput().Write(File.ReadAllBytes(filename));
    }

    static string GetImageForRequest()
    {
        try
        {
            var q = Environment.GetEnvironmentVariable("QUERY_STRING") ?? "";
            if (q == "latest")
            {
                return GetLatestImage();
            }
            if (AllDigits.IsMatch(q))
            {
                var comicId = Convert.ToInt32(q);
                if (GetComicIds().Contains(comicId))
                {
                    return GetSpecificImage(comicId);
                }
            }
        }
        catch (Exception)
        {
            // ignored
        }

        //if nothing hit, pick a random image
        return GetRandomImage();
    }

    static string GetRandomImage()
    {
        var comicIDs = GetComicIds();
        
        Random rand = new Random();
        int index = rand.Next(comicIDs.Count);
        return GetSpecificImage(comicIDs[index]);
    }

    static string GetSpecificImage(int comicId)
        => $"{ArchivePath}{comicId}.png";

    static string GetLatestImage()
        => GetSpecificImage(GetComicIds().Max());

    static List<int> GetComicIds()
        => Directory.GetFiles(ArchivePath, "*.png")
            .Select(x => Path.GetFileNameWithoutExtension(x))
            .Where(x => AllDigits.IsMatch(x))
            .Select(x => Convert.ToInt32(x)).ToList();
}
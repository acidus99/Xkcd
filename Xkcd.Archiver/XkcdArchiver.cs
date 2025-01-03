using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xkcd.Api;

namespace Xkcd.Archiver;

public class XkcdArchiver
{
    //IDs we want to skip because they are purposely left blank or are special features
    /*
     * 404 - purposely missing
     * 1190 - Time
     * 1335 -
     * 1608 - hoverboard game
     * 1663 - garden
     */
    private readonly int[] _idsToSkip = {404, 1190, 1335, 1608, 1663 };

    public string ArchiveDirectory { get; private set; }

    public XkcdArchiver(string archiveDir)
    {
        ArchiveDirectory = archiveDir;
        SetupArchive();
    }

    private void SetupArchive()
    {
        if(!ArchiveDirectory.EndsWith(Path.DirectorySeparatorChar))
        {
            ArchiveDirectory += Path.DirectorySeparatorChar;
        }
        Directory.CreateDirectory(ArchiveDirectory);
    }

    /// <summary>
    /// Does the comic exist in the archive directory?
    /// </summary>
    /// <param name="comicId"></param>
    /// <returns></returns>
    private bool IsComicArchived(int comicId)
        => File.Exists(JsonFilename(comicId)) &&
           File.Exists(ImageFilename(comicId));

    public void ArchiveAllComics()
    {
        int latestId = LatestPublishedComicId();
            
        Parallel.ForEach(Enumerable.Range(1, latestId), ArchiveComic);
    }

    private void ArchiveComic(int comicId)
    {
        try
        {
            if (IsSkippedComic(comicId) || IsComicArchived(comicId))
            {
                return;
            }
            //using local client since this can happen in parallel
            var client = new XkcdClient();

            var json = client.GetComicJson(comicId);
            var xkcdResponse = XkcdApiResponse.FromJson(json);
            var imageBytes = client.GetComicImage(xkcdResponse.ImageUrl);

            File.WriteAllText(JsonFilename(comicId), json);
            File.WriteAllBytes(ImageFilename(comicId), imageBytes);
            Console.WriteLine($"Archived: {comicId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=========== Failed to Archive: {comicId}: {ex.Message}");
        }
    }

    /// <summary>
    /// gets the latest ID currently published on XKCD.com
    /// </summary>
    /// <returns></returns>
    private int LatestPublishedComicId()
    {
        XkcdClient client = new XkcdClient();
        return client.GetLatestComicId();
    }

    /// <summary>
    /// Is the comic ID one that purposely doesn't exist, or does strange things (e.g. time)
    /// </summary>
    private bool IsSkippedComic(int comicId)
        => _idsToSkip.Contains(comicId);

    private string JsonFilename(int comicId)
        => $"{ArchiveDirectory}{comicId}.json";

    private string ImageFilename(int comicId)
        => $"{ArchiveDirectory}{comicId}.png";
}
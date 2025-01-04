using System;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using Xkcd.Api;

namespace Xkcd.Archiver;

internal static class Program
{
    private static void Main(string[] args)
    {
        Log("Starting");
        string outDir = (args.Length > 0) ? args[0] : "";

        if(outDir == "")
        {
            Log("Missing output directory. Pass archive directory as an argument");
            return;
        }
            
        Log($"Building archive in '{outDir}' directory");

        var archiver = new XkcdArchiver(outDir);
        archiver.ArchiveAllComics();

        CreateIndex(archiver.ArchiveDirectory);
    }

    private static void CreateIndex(string outDir)
    {
        Log("Creating index");
        var fout = new StreamWriter(outDir + "index.gmi");
        fout.WriteLine("# XKCD Archive");
        fout.WriteLine($"Last updated: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")} GMT");
        var numbers = Directory.GetFiles(outDir, "*.json")
            .Select(x => Convert.ToInt32(Path.GetFileNameWithoutExtension(x)))
            .OrderByDescending(x => x);

        string previousYear = DateTime.MaxValue.Year.ToString();
        foreach(var num in numbers)
        {
            var meta = FromFile(outDir + num + ".json");

            if (meta.Year != previousYear)
            {
                fout.WriteLine();
                fout.WriteLine($"## {meta.Year}");
            }
            previousYear = meta.Year;
            
            fout.WriteLine($"=> /cgi-bin/xkcd.cgi?{num} {meta.Title}  - (#{meta.ComicId} {meta.Date})");
        }
        fout.Close();
    }

    private static XkcdApiResponse FromFile(string filename)
    {
        return XkcdApiResponse.FromJson(File.ReadAllText(filename));
    }

    private static void Log(string msg)
    {
        Console.WriteLine(msg);
    }
}
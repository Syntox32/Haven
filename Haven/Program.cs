using System;
using System.IO;

using Haven.Core;

namespace Haven
{
    /// <summary>
    /// Fully working example program using the Wallhaven class
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Wallhaven[Alpha] Downloader");
            //Console.WriteLine("============================");

            // Console.WriteLine("Loading config...");

            // string json = File.ReadAllText("haven.json");

            // Console.WriteLine("Config loaded successfully...");

            
            // if (!Directory.Exists(settings.SaveLocation))
            //    Directory.CreateDirectory(settings.SaveLocation);

            //var haven = new Wallhaven(settings);
           // haven.CompleteHandler += Haven_CompleteHandler;

            /*Action callback = () =>
            {
                Console.WriteLine("\nProcess completed - Download qeue is empty");
                Console.WriteLine("Wallpapers downloaded: {0}, Errors: {1}", haven.GetWallpaperCount, haven.Errors);
                Console.WriteLine("Operation executed in seconds: {0}s", haven.DownloadTime);

                Console.WriteLine(Environment.NewLine + "Press any key to continue...");
                Console.ReadKey();
            };*/

            //Console.WriteLine(Environment.NewLine + "Preparing to download..." + Environment.NewLine);

            //haven.StartDownload();

            Console.ReadKey();
        }
    }
}
using System;
using Newtonsoft.Json;

namespace Haven
{
    /// <summary>
    /// Fully working example program using the Wallhaven class
    /// </summary>
    class Program
    {
        struct Settings
        {
            public string SaveLocation { get; set; }
            public string Url { get; set; }
            public int Pages { get; set; }
            public int PageOffset { get; set; }
        }

        static string config = @"
        {
	        'SaveLocation': 'images/',
	        'Url': 'http://alpha.wallhaven.cc/wallpaper/search?categories=101&purity=100&sorting=relevance&order=desc',
	        'Pages': 2,
	        'PageOffset': 0
        }";

        static void Main(string[] args)
        {
            Console.WriteLine("Wallhaven[Alpha] Downloader");
            Console.WriteLine("============================");

            Console.WriteLine("Loading config...");
            Settings settings = JsonConvert.DeserializeObject<Settings>(config);
            Console.WriteLine("Config loaded successfully...");

            var haven = new Wallhaven(settings.Url);

            Action callback = () =>
            {
                Console.WriteLine("\nProcess completed - Download qeue is empty");
                Console.WriteLine("Wallpapers downloaded: {0}, Errors: {1}", haven.GetWallpaperCount, haven.Errors);
                Console.WriteLine("Operation executed in seconds: {0}s", haven.DownloadTime);

                Console.WriteLine(Environment.NewLine + "Press any key to continue...");
                Console.ReadKey();
            };

            Console.WriteLine(Environment.NewLine + "Preparing to download..." + Environment.NewLine);

            haven.StartDownload(
                settings.SaveLocation,
                settings.Pages,
                settings.PageOffset,
                callback
            );

            Console.ReadKey();
        }
    }
}
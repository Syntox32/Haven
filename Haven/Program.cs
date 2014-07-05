using System;
using System.IO;
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
            public int MinHeight { get; set; }
            public int MinWidth { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        static string config = @"
        {
            'SaveLocation': 'images/',
            'Url': 'http://alpha.wallhaven.cc/wallpaper/search?categories=100&purity=100&sorting=favorites&order=desc',
            'Pages': 1,
            'PageOffset': 0,
            'MinWidth': 1200,
            'MinHeight': 1920,
            'Username': '',
            'Password': ''
        }";

        static void Main(string[] args)
        {
            Console.WriteLine("Wallhaven[Alpha] Downloader");
            Console.WriteLine("============================");

            Console.WriteLine("Loading config...");
            Settings settings = JsonConvert.DeserializeObject<Settings>(config);
            Console.WriteLine("Config loaded successfully...");

            if (!Directory.Exists(settings.SaveLocation))
                Directory.CreateDirectory(settings.SaveLocation);

            var haven = new Wallhaven(settings.Url, settings.Username, settings.Password, false)
            {
                Savepath = settings.SaveLocation,
                MinimumWidth = settings.MinWidth,
                MinimumHeight = settings.MinHeight
            };

            Action callback = () =>
            {
                Console.WriteLine("\nProcess completed - Download qeue is empty");
                Console.WriteLine("Wallpapers downloaded: {0}, Errors: {1}", haven.GetWallpaperCount, haven.Errors);
                Console.WriteLine("Operation executed in seconds: {0}s", haven.DownloadTime);

                Console.WriteLine(Environment.NewLine + "Press any key to continue...");
                Console.ReadKey();
            };

            Console.WriteLine(Environment.NewLine + "Preparing to download..." + Environment.NewLine);

            haven.DownloadCompleteCallback = callback;
            haven.StartDownload(settings.Pages, settings.PageOffset);

            Console.ReadKey();
        }
    }
}
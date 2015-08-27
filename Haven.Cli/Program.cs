using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Haven.Core;
using System.IO;

namespace Haven.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Wallhaven[Alpha] Batch downloader\n");
            Console.WriteLine("Project: https://github.com/Syntox32/Haven");

            string config = string.Empty;

            if (args.Length == 0)
            {
                Console.WriteLine("Trying to locate config..");

                var ret = TryLocateConfig();
                if (ret == null)
                {
                    Console.WriteLine("config could not be located.\n");

                    PrintUsage();
                    Console.ReadKey();
                    Environment.Exit(1);
                }
                else
                    config = ret;
            }
            else
                config = args[0];

            if (!File.Exists(config))
            {
                Console.WriteLine("File does not exist:\n   {0}", config);
                Console.ReadKey();
                Environment.Exit(1);
            }

            Console.WriteLine("\nConfig: {0}\n", config);

            var haven = new Wallhaven(config);

            haven.CompleteHandler += DownloadComplete;
            haven.StartDownload();

            Console.ReadKey();
        }

        private static void DownloadComplete(object sender, HavenEventArgs e)
        {
            var h = sender as Wallhaven;
            Console.WriteLine("Download completed in {0} seconds", h.DownloadTime);
            Console.WriteLine("Downloaded {0} wallpapers", h.GetDownloadCount);
            Console.WriteLine("{0} error(s) occured", h.Errors);

            Console.WriteLine("Done.");
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:\n   haven.exe <config-path>");
        }

        private static string TryLocateConfig()
        {
            var curr = Environment.CurrentDirectory;
            var files = Directory.GetFiles(curr);

            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                if (name == "config.yaml")
                    return file;
            }

            return null;
        }
    }
}

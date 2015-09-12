using System;
using System.IO;
using YamlDotNet.Serialization;

namespace Haven.Core
{
    public static class Config
    { 
        public static Settings LoadYamlConfig(string path)
        {
            // i just noticed how little support .net
            // has for yaml, god damnit
            var reader = File.OpenText(path);

            var deserializer = new Deserializer(ignoreUnmatched: true);
            var settings = deserializer.Deserialize<Settings>(reader);

            return settings;
        }
    }

    public class Settings
    {
        [YamlMember(Alias = "save_location")]
        public string SaveLocation { get; set; }

        [YamlMember(Alias = "url")]
        public string Url { get; set; }

        [YamlMember(Alias = "pages")]
        public int Pages { get; set; }

        [YamlMember(Alias = "threads")]
        public int Threads { get; set; }

        [YamlMember(Alias = "page_offset")]
        public int PageOffset { get; set; }

        [YamlMember(Alias = "use_min")]
        public bool UseMin { get; set; }

        [YamlMember(Alias = "min_height")]
        public int MinHeight { get; set; }

        [YamlMember(Alias = "min_width")]
        public int MinWidth { get; set; }

        [YamlMember(Alias = "use_auth")]
        public bool UseAuth { get; set; }

        [YamlMember(Alias = "username")]
        public string Username { get; set; }

        [YamlMember(Alias = "password")]
        public string Password { get; set; }
    }
}

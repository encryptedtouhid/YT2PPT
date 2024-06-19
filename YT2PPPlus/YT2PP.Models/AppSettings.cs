using System;

namespace YT2PP.Models
{
    public class AppSettings
    {

        public string Version { get; set; }

        public string YouTubeApiKey { get; set; }

        public TimeSpan FreeLimit { get; set; }
    }
}

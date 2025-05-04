using System;

namespace YT2PP.Models
{
    public class AppSettings
    {
        public string Version { get; set; } = string.Empty;

        public string YouTubeApiKey { get; set; } = string.Empty;

        public TimeSpan FreeLimit { get; set; }

        public string AppOwner { get; set; } = string.Empty;

        public string AppOwnerUrl { get; set; } = string.Empty;

        public string AppCode { get; set; } = string.Empty;

        public string AppName { get; set; } = string.Empty;
    }
}

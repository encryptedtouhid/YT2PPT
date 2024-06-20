using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace YT2PP.Common
{
    public class AppUtilities
    {
        public string GetYouTubeVideoId(string url)
        {
            string pattern = "(?<=watch\\?v=|/videos/|embed\\/|youtu.be\\/|\\/v\\/|\\/e\\/|watch\\?v%3D|watch\\?feature=player_embedded&v=|%2Fvideos%2F|embed%\u200C\u200B2F|youtu.be%2F|\\/v%2F|e%2F|watch\\?v=|&v=|\\?v=)([^#\\&\\?\\n]*[^\\&\\?\\n])";
            Match match = Regex.Match(url, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }
    }
}

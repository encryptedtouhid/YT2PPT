using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using YT2PP.Services.Interfaces;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YT2PP.Models;
using System.Xml;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;



namespace YT2PP.Services.Implementations
{
    public class YTService : IYTService
    {
        private readonly AppSettings _appSettings;
        public YTService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public async Task<string> GetStreamUrlAsync(string videoUrl)
        {
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);
            var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

            return streamInfo.Url;
        }

        public bool ValidateVideoLength(string videoId)
        {
            bool IsOK = false;
            var videoTime = this.GetYouTubeVideoDuration(videoId);
            if (videoTime <= _appSettings.FreeLimit && videoTime != TimeSpan.Zero)
            {
                IsOK = true;
            }
            return IsOK;
        }

        private TimeSpan GetYouTubeVideoDuration(string videoId)
        {
            try
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = this._appSettings.YouTubeApiKey,
                    ApplicationName = this._appSettings.AppName
                });

                var listRequest = youtubeService.Videos.List("contentDetails");
                listRequest.Id = videoId;

                var response = listRequest.Execute();
                var video = response.Items[0];

                string durationStr = video.ContentDetails.Duration;
                TimeSpan duration = XmlConvert.ToTimeSpan(durationStr);

                return duration;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching YouTube video duration: {ex.Message}");
                return TimeSpan.Zero;
            }
        }

        public string GetYouTubeVideoId(string url)
        {
            string pattern = "(?<=watch\\?v=|/videos/|embed\\/|youtu.be\\/|\\/v\\/|\\/e\\/|watch\\?v%3D|watch\\?feature=player_embedded&v=|%2Fvideos%2F|embed%\u200C\u200B2F|youtu.be%2F|\\/v%2F|e%2F|watch\\?v=|&v=|\\?v=)([^#\\&\\?\\n]*[^\\&\\?\\n])";
            Match match = Regex.Match(url, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

   
        private static void DeleteDuplicateImages(string directoryPath)
        {
            // Get all image files from the directory
            string[] imageFiles = Directory.GetFiles(directoryPath, "*.png");

            // Dictionary to store image hashes
            Dictionary<string, string> imageHashes = new Dictionary<string, string>();

            foreach (var imageFile in imageFiles)
            {
                string hash = GetImageHash(imageFile);

                // If the hash already exists, delete the file
                if (imageHashes.ContainsValue(hash))
                {
                    Console.WriteLine($"Duplicate found: {imageFile}");
                    File.Delete(imageFile);
                }
                else
                {
                    imageHashes.Add(imageFile, hash);
                }
            }
        }
        // Function to compute the hash of an image file
        private static string GetImageHash(string imagePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(imagePath))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public void ExtractFramesFromStreamAsync(string streamUrl, string outputDirectory)
        {

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{streamUrl}\" -vf \"select='eq(n\\,0)+gt(scene,0.01)',fps=1\" -vsync vfr {Path.Combine(outputDirectory, "frame_%03d.png")}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            ffmpegProcess.Start();
            ffmpegProcess.WaitForExit(); // Await the process to finish

            DeleteDuplicateImages(outputDirectory);
        }       
    }
}

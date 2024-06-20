using System.Threading.Tasks;
using YT2PP.Services.Interfaces;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using Microsoft.Extensions.Options;
using YT2PP.Models;
using System;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.Xml;

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
    }
}

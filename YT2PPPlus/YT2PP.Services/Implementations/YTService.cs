using System.Threading.Tasks;
using YT2PP.Services.Interfaces;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YT2PP.Services.Implementations
{
    public class YTService : IYTService
    {
        public async Task<string> GetStreamUrlAsync(string videoUrl)
        {
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);
            var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

            return streamInfo.Url;
        }
    }
}

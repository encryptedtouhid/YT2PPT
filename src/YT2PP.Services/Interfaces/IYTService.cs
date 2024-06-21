using System.Threading.Tasks;

namespace YT2PP.Services.Interfaces
{
    public interface IYTService
    {
        Task<string> GetStreamUrlAsync(string videoUrl);
        string GetYouTubeVideoId(string url);
        bool ValidateVideoLength(string videoId);
        void ExtractFramesFromStreamAsync(string streamUrl, string outputDirectory);
    }

}

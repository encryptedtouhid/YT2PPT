using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using YT2PP.Services.Interfaces;
using YT2PP.Web.Models;


namespace YT2PP.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IYTService _iYTService;
        private readonly IPPTService _pPTService;
        public HomeController(ILogger<HomeController> logger, IYTService yTService, IPPTService pPTService)
        {
            _logger = logger;
            _iYTService = yTService;
            _pPTService = pPTService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Extract(DataInputViewModel model)
        {
            string streamUrl = string.Empty;
            string videoId = string.Empty;


            if (model != null)
            {
                string VideoURL = model.DataInput;
                videoId = _iYTService.GetYouTubeVideoId(VideoURL);

                if (_iYTService.ValidateVideoLength(videoId))
                {
                    string frameOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "Output", "Frames", videoId);
                    string pptOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "Output", "Powerpoints", videoId + ".ppt");
                    try
                    {
                        var Streamtask = _iYTService.GetStreamUrlAsync(VideoURL);
                        Streamtask.Wait();
                        streamUrl = Streamtask.Result;
                        
                        _iYTService.ExtractFramesFromStreamAsync(streamUrl, frameOutputPath);
                        
                        _pPTService.CreatePresentation(pptOutputPath, frameOutputPath);
                        // Read the PowerPoint file into a byte array
                        byte[] pptBytes = System.IO.File.ReadAllBytes(pptOutputPath);
                        // Return the byte array as a file download
                        return File(pptBytes, "application/octet-stream", videoId + ".pptx");
                    }
                    catch (AggregateException ex)
                    {
                        throw ex.InnerExceptions.First();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.StackTrace.ToString());
                        return BadRequest("Failed to extract PowerPoint: " + ex.Message);
                    }
                }
                else
                {

                }               
            }
            return View();
        }
    }
}

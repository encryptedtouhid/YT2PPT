using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.Collections;
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
        private readonly IToastNotification _toastNotification;

        public HomeController(ILogger<HomeController> logger, IYTService yTService, IPPTService pPTService, IToastNotification toastNotification)
        {
            _logger = logger;
            _iYTService = yTService;
            _pPTService = pPTService;
            _toastNotification = toastNotification;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            //Success
            _toastNotification.AddSuccessToastMessage("Same for success message");
            // Success with default options (taking into account the overwritten defaults when initializing in Startup.cs)
            _toastNotification.AddSuccessToastMessage();

            //Info
            _toastNotification.AddInfoToastMessage();

            //Warning
            _toastNotification.AddWarningToastMessage();

            //Error
            _toastNotification.AddErrorToastMessage();
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
            string VideoURL = model.DataInput;
            videoId = _iYTService.GetYouTubeVideoId(VideoURL);

            if (_iYTService.ValidateVideoLength(videoId))
            {

                string frameOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "Output", "Frames", videoId);
                string pptOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "Output", "Powerpoints", videoId + ".pptx");
                try
                {
                    var Streamtask = _iYTService.GetStreamUrlAsync(VideoURL);
                    Streamtask.Wait();
                    streamUrl = Streamtask.Result;

                    _iYTService.ExtractFramesFromStreamAsync(streamUrl, frameOutputPath);

                    _pPTService.CreatePresentation(pptOutputPath, frameOutputPath);

                    byte[] pptBytes = System.IO.File.ReadAllBytes(pptOutputPath);
                   
                    return File(pptBytes, "application/vnd.openxmlformats-officedocument.presentationml.presentation", videoId + ".pptx");
                }
                catch (AggregateException ex)
                {
                    throw ex.InnerExceptions.First();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.StackTrace.ToString());
                    string message = $"Failed to extract PowerPoint. Try Again Later.";
                    _toastNotification.AddErrorToastMessage(message);
                    return BadRequest("Failed to extract PowerPoint: " + ex.Message);
                }
            }
            else
            {

                string message = $"Cannot convert the YouTube video because it is longer than 15 minutes";
                _toastNotification.AddErrorToastMessage(message);
                return RedirectToAction("Index");
            }

        }
         
    }
}

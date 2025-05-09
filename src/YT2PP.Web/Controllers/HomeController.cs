using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
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
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public JsonResult Extract(DataInputViewModel model)
        {
            string videoId = string.Empty;
            string VideoURL = model.DataInput;
            ReturnVm returnVm = new ReturnVm();
            returnVm.RequestId = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(VideoURL))
            {
                videoId = _iYTService.GetYouTubeVideoId(VideoURL);
                returnVm.VId = videoId;
            }           
          
            if (_iYTService.ValidateVideoLength(videoId))
            {

                string frameOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "Output", "Frames", videoId);
                string pptOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "Output", "Powerpoints", videoId + ".pptx");
                try
                {
                    var streamUrl = _iYTService.GetStreamUrlAsync(VideoURL).Result;

                    _iYTService.ExtractFramesFromStreamAsync(streamUrl, frameOutputPath);

                    _pPTService.CreatePresentation(pptOutputPath, frameOutputPath);


                    returnVm.IsSuccess = true;
                    returnVm.IsAvailableDownload = true;
                    _toastNotification.AddSuccessToastMessage("Conversion Success!");
                }
                catch (AggregateException ex)
                {
                    returnVm.IsSuccess = false;
                    _logger.LogError(ex.ToString());
                    string messageExp = $"Failed to extract PowerPoint. Try Again Later.";
                    _toastNotification.AddErrorToastMessage(messageExp);
                  
                }
                catch (Exception ex)
                {
                    returnVm.IsSuccess = false;
                    _logger.LogError(ex.ToString());
                    string messageExp2 = $"Failed to extract PowerPoint. Try Again Later.";
                    _toastNotification.AddErrorToastMessage(messageExp2);
                   
                }
                
            }
            else
            {
                returnVm.IsSuccess = false;
                string message = $"Cannot convert the YouTube video because it is longer than 15 minutes";
                _toastNotification.AddErrorToastMessage(message);               

            }

            return Json(returnVm);
        }
         
        public ActionResult DownloadFile(string id)
        {
            string pptOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "Output", "Powerpoints", id + ".pptx");
            byte[] pptBytes = System.IO.File.ReadAllBytes(pptOutputPath);
            return File(pptBytes, "application/octet-stream", id + ".pptx");
        }

    }
}

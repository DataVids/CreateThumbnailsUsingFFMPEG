using DataVidsThumbnails.Models;
using DataVidsThumbnails.Services.Abstract;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DataVidsThumbnails.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISetupService _setupService;
        private static readonly List<string> VideoExtensions = new List<string> { ".mov", ".avi", ".mpg", ".mpeg", ".mp4", ".webm", ".ogg" };

        public HomeController(ILogger<HomeController> logger, ISetupService setupService)
        {
            _logger = logger;
            _setupService = setupService;
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
        public async Task<ActionResult<VideoUpload>> SavePicture(VideoUpload model)
        {
            
            var videoProcessResults = new ServiceResponse()
            {
                Success = true
            };
            
            try
            {
                var files = model.File; // same as Request.Form.Files;


                foreach (var file in files)
                {
                    if (VideoExtensions.Contains(Path.GetExtension(file.FileName)))
                    {
                        videoProcessResults = await _setupService.UploadVideosAsync(file, model.Title);
                    }
                    else
                    {
                        throw new ApplicationException("Invalid video type, please upload another format.");
                    }
                }
            }
            catch(Exception ex)
            {
                videoProcessResults.Success = false;
                videoProcessResults.WarningText = ex.Message;
                //probably want to LOG the stack trace here. 
            }
            return View("Index", new VideoUpload() { Results = videoProcessResults });
        }
    }
}
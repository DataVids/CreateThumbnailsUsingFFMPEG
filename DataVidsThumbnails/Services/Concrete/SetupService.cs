using DataVidsThumbnails.Models;
using DataVidsThumbnails.Services.Abstract;
using Microsoft.Extensions.Options;
using Microsoft.Win32.SafeHandles;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace DataVidsThumbnails.Services.Concrete
{
    
    public class SetupService : ISetupService
    {
#pragma warning disable CA1416 // Validate platform compatibility (windows only)

        private readonly IImageService _imageService;
        private readonly AppSettings _appSettings;

        private static Random random = new Random();
        private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        #region constructors
        public SetupService(
            IImageService imageService,
            IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _imageService = imageService;
        }
        #endregion
        #region public methods

        
        public async Task<ServiceResponse> UploadVideosAsync(IFormFile file, string title)
        {
            ServiceResponse result = new ServiceResponse();
            if (file.Length <= 0)
            {
                result.Success = false;
                return result;
            }
            try
            {
                var videoDetails = await _imageService.ProcessVideo(file);

                

                //here you might want to save videoDetails to the database.. Info about the image you processed.
            }
            catch(Exception ex)
            {
                result.WarningText = ex.Message;
                //todo: log here or throw back up to utilize the rest of ex
                result.Success = false;
                return result;
            }

            result.Success = true;
            return result;
        }



        #endregion
    }
}

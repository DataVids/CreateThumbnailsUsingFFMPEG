using DataVidsThumbnails.Models;

namespace DataVidsThumbnails.Services.Abstract
{
    public interface ISetupService
    {
        Task<ServiceResponse> UploadVideosAsync(IFormFile file, string title);
    }
}

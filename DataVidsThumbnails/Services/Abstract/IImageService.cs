using DataVidsThumbnails.Models;

namespace DataVidsThumbnails.Services.Abstract
{
    public interface IImageService
    {
        Task<VideoDetails> ProcessVideo(IFormFile file);
    }
}

using System.ComponentModel;

namespace DataVidsThumbnails.Models
{
    public class VideoUpload
    {
        [DisplayName("Datavids Video File")]
        public IList<IFormFile> File { get; set; }
        [DisplayName("Datavids Video Title")]
        public string Title { get; set; }

        public ServiceResponse Results { get; set; }
    }
}

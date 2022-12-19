using System.Drawing;

namespace DataVidsThumbnails.Models
{
    public class VideoDetails
    {
        public Bitmap Thumbnail { get; set; }
        public TimeSpan Length { get; set; }
        public string Height { get; set; }
        public string Width { get; set; }
        public string ContentType { get; set; }
        public string SavedVideoPath { get; set; }
        public string SavedThumbVideoPath { get; set; }
    }
}

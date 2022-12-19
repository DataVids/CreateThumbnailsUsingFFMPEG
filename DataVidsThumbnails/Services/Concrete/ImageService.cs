using DataVidsThumbnails.Models;
using DataVidsThumbnails.Services.Abstract;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;

namespace DataVidsThumbnails.Services.Concrete
{
    public class ImageService : IImageService
    {
        private readonly AppSettings _appSettings;
        private static Random random = new Random();
        public ImageService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        #region public methods

        public async Task<VideoDetails> ProcessVideo(IFormFile file)
        {
            if (file.Length > 0)
            {
                var guid = Guid.NewGuid();
                var thumbPath = "thumb_" + guid;
                var videoPath = "video_" + guid;

                var partialPath = Guid.NewGuid().ToString().Replace("-", "");

                var thumbFname = thumbPath + partialPath + ".BMP";
                var videoFname = videoPath + partialPath + Path.GetExtension(file.FileName);

                var thumbFilePath = Path.Combine(Path.GetTempPath(), thumbFname);
                var videoFilePath = Path.Combine(Path.GetTempPath(), videoFname);

                //save the video file first:

                using Stream fileStream = new FileStream(videoFilePath, FileMode.Create);
                await file.CopyToAsync(fileStream);

                var fi = new FileInfo(file.FileName);
                string extension = fi.Extension.ToUpper();
                if (extension != ".MP4" &&
                    extension != ".WEBM" &&
                    extension != ".OGG")
                {
                    videoFilePath = ConvertToMP4(videoFilePath);
                    extension = ".MP4";
                }
                string contentType = "";
                switch (extension)
                {
                    case ".MP4":
                        contentType = "video/MP4";
                        break;
                    case ".WEBM":
                        contentType = "video/WebM";
                        break;
                    case ".OGG":
                        contentType = "video/Ogg";
                        break;
                }

                var length = await GetVideoLength(videoFilePath);
                var heightWidthStr = await GetVideoHeightWidth(videoFilePath);
                var heightWidth = heightWidthStr.Split('x');

                string width = heightWidth[0];
                string height = heightWidth[1];

                //get rounded ints for use in ratio (for thumbnail sizes only):
                int nWidth = Convert.ToInt32(width);
                int nHeight = Convert.ToInt32(height);
                int ratioHeight = 0;
                int ratioWidth = 0;
                ApplyRatio(nWidth, nHeight, ref ratioWidth, ref ratioHeight);

                //get thumbnail:
                var snapshotResult = await FFMpeg
                .SnapshotAsync(
                    videoFilePath,
                    new Size(ratioWidth, ratioHeight),
                    null//TimeSpan.FromMinutes(1) //apply this if we want the thumb from a later point in time in the vid
                    );

                using MemoryStream thumbMemStream = new MemoryStream();
                snapshotResult.Save(thumbMemStream, System.Drawing.Imaging.ImageFormat.Bmp); //Install nuget: System.Drawing.Common v6
                thumbMemStream.Position = 0;

                using Stream thumbFileStream = new FileStream(thumbFilePath, FileMode.Create);
                await thumbMemStream.CopyToAsync(thumbFileStream);

                var result = new VideoDetails()
                {
                    ContentType = contentType,
                    Width = width,
                    Height = height,
                    Length = length,
                    SavedVideoPath = videoFilePath,
                    SavedThumbVideoPath = thumbFilePath,
                    Thumbnail = snapshotResult,
                };

                return result;
            }
            else
            {
                throw new ApplicationException("Invalid file, please try another video file or a different video format.");
            }
        }


        #endregion

        #region private methods

        private string ConvertToMP4(string inputPath)
        {
            //(first, install FFMpegCore nuget package)

            string outputPath = Path.GetFileNameWithoutExtension(inputPath) + ".mp4";
            FFMpegArguments
            .FromFileInput(inputPath)
            .OutputToFile(outputPath, false, options => options
                .WithVideoCodec(VideoCodec.LibX264)
                .ForceFormat("mp4")
                .WithConstantRateFactor(21)
                .WithAudioCodec(AudioCodec.Aac)
                .WithVariableBitrate(4)
                .WithVideoFilters(filterOptions => filterOptions
                    .Scale(VideoSize.Hd))
                .WithFastStart())
            .ProcessSynchronously();
            return outputPath;
        }

      
        private void ApplyRatio(int width, int height, ref int newWidth, ref int newHeight)
        {
            int maxWidth = _appSettings.DEFAULT_IMAGE_WIDTH;
            int maxHeight = _appSettings.DEFAULT_IMAGE_HEIGHT;

            var ratioX = (float)maxWidth / width;
            var ratioY = (float)maxHeight / height;
            var ratio = Math.Min(ratioX, ratioY);

            newWidth = (int)Math.Round(width * ratio);
            newHeight = (int)Math.Round(height * ratio);
        }

        private async Task<string> GetVideoHeightWidth(string filePath)
        {
            string cmd = string.Format("-v error -select_streams v -show_entries stream=width,height -of csv=p=0:s=x {0}", filePath);
            Process theProcess = new Process();
            theProcess.StartInfo.FileName = "./ffmpeg/bin/ffprobe.exe";
            theProcess.StartInfo.Arguments = cmd;
            theProcess.StartInfo.CreateNoWindow = true;
            theProcess.StartInfo.RedirectStandardOutput = true;
            theProcess.StartInfo.RedirectStandardError = true;
            theProcess.StartInfo.UseShellExecute = false;
            theProcess.StartInfo.UseShellExecute = false;
            if (!theProcess.Start())
            {
                Debug.WriteLine("Could not start the FFMpeg process!");
                return "0x0"; //failed
            }
            string result = theProcess.StandardOutput.ReadToEnd();
            theProcess.WaitForExit();
            theProcess.Close();
            return result;
        }

        private async Task<TimeSpan> GetVideoLength(string filepath)
        {
            //make sure you have downloaded from ffmpeg site the binaries and put in this folder!

            string ffMPEG = "./ffmpeg/bin/ffMPEG.exe";
            string outPut = "";
            string param = string.Format("-i \"{0}\"", filepath);
            ProcessStartInfo processStartInfo = null;

            //add using 'System.Text.RegularExpressions' for regex commands!

            Regex regex = null;
            Match m = null;
            TimeSpan duration = TimeSpan.MinValue;

            //Get ready with ProcessStartInfo
            processStartInfo = new ProcessStartInfo(ffMPEG, param);
            processStartInfo.CreateNoWindow = true;

            //ffMPEG uses StandardError for its output.
            processStartInfo.RedirectStandardError = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.UseShellExecute = false;

            Process systemProcess = Process.Start(processStartInfo);
            if (systemProcess != null)
            {
                StreamReader streamReader = systemProcess.StandardError;

                outPut = streamReader.ReadToEnd();
                systemProcess.WaitForExit();
                systemProcess.Close();
                systemProcess.Dispose();
                streamReader.Close();
                streamReader.Dispose();

                //get duration

                regex = new Regex("[D|d]uration:.((\\d|:|\\.)*)");
                m = regex.Match(outPut);

                if (m.Success)
                {
                    string temp = m.Groups[1].Value;
                    string[] timepieces = temp.Split(new char[] { ':', '.' });
                    if (timepieces.Length == 4)
                    {
                        duration = new TimeSpan(0, Convert.ToInt16(timepieces[0]), Convert.ToInt16(timepieces[1]), Convert.ToInt16(timepieces[2]), Convert.ToInt16(timepieces[3]));
                    }
                }
                return duration;

            }
            return new TimeSpan(0);//failed to start process
        }

        #endregion



    }
}

using System;

namespace SweetHome.Editor.Models
{
    [Serializable]
    public class ScreenshotOutput
    {
        public string filePath;
        public string error;
        public string stackTrace;
        public string base64Image;
    }
}
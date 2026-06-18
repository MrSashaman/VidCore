using System;
using System.Collections.Generic;

namespace VidCore
{
    public class Video
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public bool IsPrivate { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public DateTime UploadDate { get; set; }
        public string VideoPath { get; set; }
        public string ThumbnailPath { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace VidCore
{
    public class Comment
    {
        public int id { get; set; }
        public string Text { get; set; }
        public DateTime UploadDate { get; set; }
        public int VideoId { get; set; }
        public string Author { get; set; }
    }
}
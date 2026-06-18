using System;
using System.Collections.Generic;

namespace VidCore
{
    public class Music
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Likes { get; set; }
        public int Views { get; set; }
        public DateTime UploadDate { get; set; }
        public string AudioPath { get; set; }
        public string Artist { get; set; }
        public string Genre { get; set; }
        public int DurationSeconds { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
namespace Pr0gramm.Model
{
    using System;
    using System.Threading;

    public class Post
    {
        public int Id;
        public int Promoted;
        public int Up { get; set; }
        public int Down { get; set; }
        public int Created { get; set; }
        public DateTime ReadableCreatedTime { get; set; }
        public string Image { get; set; }
        public string Thumb { get; set; }
        public string Fullsize;
        public string Source;
        public string User { get; set; }
        public int Mark { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}

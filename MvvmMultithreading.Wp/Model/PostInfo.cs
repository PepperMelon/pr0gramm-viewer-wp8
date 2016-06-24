using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Model
{
    using System.Collections.ObjectModel;
    using BaseClasses;
    using Extensions;
    using Newtonsoft.Json;

    public class PostInfo : ExtendedViewModelBase
    {
        public ObservableCollection<TagInfo> Tags
        {
            get
            {
                return tags;
            }
            set
            {
                tags = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<Comment> Comments
        {
            get
            {
                return comments;
            }
            set
            {
                comments = value;
                this.OnPropertyChanged();
            }
        }

        public int Ts;
        public string Cache;
        public int Rt;
        public int Qc;
        private ObservableCollection<Comment> comments;
        private ObservableCollection<TagInfo> tags;

        public static PostInfo FromJson(string json)
        {
            return JsonConvert.DeserializeObject<PostInfo>(json);
        }
    }

    public class TagInfo
    {
        public int Id;
        public double Confidence;
        public string Tag { get; set; }
    }

    public class Comment
    {
        private int created;

        public int Id;
        public int Parent { get; set; }

        public int? Level { get; set; }

        public string Content { get; set; }

        public int Created
        {
            get
            {
                return created;
            }
            set
            {
                created = value;
                ReadableCreatedTime = Helper.ConvertJsonDateToDateTime(value).AddHours(2);
            }
        }
        
        public DateTime ReadableCreatedTime { get; set; }
        public int Up { get; set; }
        public int Down { get; set; }
        public double Confidence;
        public string Name { get; set; }
        public int Mark { get; set; }

        public bool IsUserOriginalPoster { get; set; }
    }
}

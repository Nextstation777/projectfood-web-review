using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace React.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        public string Topic { get; set; }
        public string Text { get; set; }
        public string CreateTime { get; set; }
        public string CoverPhoto { get; set; }
        public int ShopId { get; set; }

        public List<CommentPost>? CommentPost { get; set; }
        public List<RatingPost>? RatingPost { get; set; }
        public List<ImagePost>? ImagePost { get; set; }
        [JsonIgnore]
        public Shop Shop { get; set; }


    }
}

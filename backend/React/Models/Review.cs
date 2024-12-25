using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace React.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        public string Topic { get; set; }
        public string Text { get; set; }
        public string CreateTime { get; set; }
        public string CoverPhoto { get; set; }
        public int ?ShopId { get; set; }
        public int UserId { get; set; }



        [NotMapped] 
        public double ReviewScore { get; set; }
        public List<CommentReview>? CommentReview { get; set; }
        public List<RatingReview>? RatingReview { get; set; }
        public List<ImageReview>? ImageReview { get; set; }
        public User User { get; set; }
        [JsonIgnore]
        public Shop ?Shop { get; set; }


    }
}

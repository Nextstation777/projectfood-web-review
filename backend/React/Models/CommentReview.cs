using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace React.Models
{
    public class CommentReview
    {
        [Key]
        public int CommentReviewId { get; set; }

        public int ReviewId { get; set; }

        public int UserId { get; set; }

        public string Text { get; set; }

        [NotMapped]
        public int ScoreLike { get; set; }


        public List<LikeCommentReview>? LikeCommentReview { get; set; }


    }
}

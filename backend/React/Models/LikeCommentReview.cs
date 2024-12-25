using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace React.Models
{
    public class LikeCommentReview
    {
        [Key]
        public int LikeReviewId { get; set; }
        public int UserId { get; set; }
        public int CommentReviewId { get; set; }
        public int Like { get; set; }




    }
}

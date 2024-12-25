using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace React.Models
{
    public class LikeCommentPost
    {
        [Key]
        public int LikepostId { get; set; }
        public int UserId { get; set; }
        public int CommentPostId { get; set; }
        public int Like { get; set; }

    }
}

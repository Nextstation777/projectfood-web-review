using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace React.Models
{
    public class CommentPost
    {
        [Key]
        public int CommentPostId { get; set; }

        public int PostId { get; set; }

        public int UserId { get; set; }

        public string Text { get; set; }

        [NotMapped]
        public int ScoreLike { get; set; }


        public List<LikeCommentPost>? LikeCommentPost { get; set; }


    }
}

using System.ComponentModel.DataAnnotations;

namespace React.share
{
    public record CommentReviewCreate
    {

        public int ReviewId { get; set; }
        public string Text { get; set; }


    }
}

using System.ComponentModel.DataAnnotations;

namespace React.share
{
    public record CommentPostCreate
    {

        public int PostId { get; set; }
        public string Text { get; set; }


    }
}

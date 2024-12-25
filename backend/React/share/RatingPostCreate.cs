using System.ComponentModel.DataAnnotations;

namespace React.share
{
    public record RatingPostCreate
    {

        public int PostId { get; set; }

        [Range(1, 5, ErrorMessage = "PostScore must be between 1 and 5.")]
        public int PostScore { get; set; }

    }
}

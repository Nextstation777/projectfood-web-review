using System.ComponentModel.DataAnnotations;

namespace React.share
{
    public record ImageReviewCreate
    {
        public int ReviewId { get; set; }
        public List<IFormFile> Images { get; set; }
    }
}
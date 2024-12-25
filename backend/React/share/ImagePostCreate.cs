using System.ComponentModel.DataAnnotations;

namespace React.share
{
    public record ImagePostCreate
    {
        public int PostId { get; set; }
        public List<IFormFile> Images { get; set; }
    }
}
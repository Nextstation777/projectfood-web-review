using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

namespace React.share
{
    public class ImageReviewUpdate
    {
        public IFormFile NewImage { get; set; }
    }
}

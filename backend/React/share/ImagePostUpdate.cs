using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

namespace React.share
{
    public class ImagePostUpdate
    {
        public IFormFile NewImage { get; set; }
    }
}

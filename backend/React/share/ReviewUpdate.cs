using System.ComponentModel.DataAnnotations;

namespace React.share
{
    public record ReviewUpdate
    {

        public string Topic { get; set; }

        public string Text { get; set; }

        public IFormFile CoverPhoto { get; set; }


    }
}

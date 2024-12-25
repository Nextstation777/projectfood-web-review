using System.ComponentModel.DataAnnotations;

namespace React.share
{
    public record PostCreate
    {

        public string Topic { get; set; }

        public string Text { get; set; }

        public IFormFile CoverPhoto { get; set; }




    }
}

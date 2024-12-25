using System.ComponentModel.DataAnnotations;

namespace React.share
{
    public record ReviewCreate
    {

        public string Topic { get; set; }

        public string Text { get; set; }

        public IFormFile CoverPhoto { get; set; }

        public int? ShopId { get; set; }



    }
}

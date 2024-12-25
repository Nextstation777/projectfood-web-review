using System.ComponentModel.DataAnnotations;

namespace React.share
{
    public record ShopCreate
    {

        public string ShopName { get; set; }

        public string AddressNumber { get; set; }

        public string Detail { get; set; }

        public string District { get; set; }

        public string Province { get; set; }


    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace React.Models
{
    public class Shop
    {
        [Key]
        public int ShopId { get; set; }

        public string ShopName { get; set; }
        public string Detail { get; set; }
        public int UserId { get; set; }
        public string AddressNumber { get; set; }
        public string District { get; set; }
        public string Province { get; set; }


        public Post Post { get; set; }
        public User User { get; set; }

        [JsonIgnore]
        public List<Review>? Review { get; set; }
    }
}

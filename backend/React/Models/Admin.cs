using System.ComponentModel.DataAnnotations;
using System.Net;

namespace React.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public string State { get; set; }


    }

}

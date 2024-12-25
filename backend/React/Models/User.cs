using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace React.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string ProfilePic { get; set; }

        public string State { get; set; }

        public string TimeRegister { get; set; }


        [NotMapped]
        public int ReviewCount { get; set; }



        [JsonIgnore]
        public Shop? Shop { get; set; }

        [JsonIgnore]
        public List<Review>? Review { get; set; }
    }
}

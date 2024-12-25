using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace React.Models
{
    public class ImagePost
    {
        [Key]
        public int ImagePostId { get; set; }

        public int PostId { get; set; }

        public string Images { get; set; }

    }
}

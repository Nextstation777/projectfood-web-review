using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace React.Models
{
    public class ImageReview
    {
        [Key]
        public int ImageReviewId { get; set; }

        public int ReviewId { get; set; }

        public string Images { get; set; }

    }
}

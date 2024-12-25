using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace React.Models
{
    public class RatingReview
    {
        [Key]
        public int RatingReviewId { get; set; }

        public int ReviewId { get; set; }

        public int UserId { get; set; }

        [Range(1, 5, ErrorMessage = "PostScore must be between 1 and 5.")]
        public int ReviewScore { get; set; }


    }
}

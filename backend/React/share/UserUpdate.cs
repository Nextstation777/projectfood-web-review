using System.ComponentModel.DataAnnotations;

namespace React.share
{
    public record UserUpdate
    {

        public string Name { get; set; }

        public string Password { get; set; }

        public IFormFile? ProfilePic { get; set; }


    }
}

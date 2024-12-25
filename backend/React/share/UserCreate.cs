using System.ComponentModel.DataAnnotations;

namespace React.share
{
    public record Usercreate
    {

        public string UserName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Name { get; set; }

        public IFormFile ProfilePic { get; set; }


    }
}

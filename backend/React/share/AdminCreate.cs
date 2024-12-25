using System.ComponentModel.DataAnnotations;

namespace React.share
{
    public record AdminCreate
    {

        public string Email { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }


    }
}

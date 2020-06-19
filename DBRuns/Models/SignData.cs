using System.ComponentModel.DataAnnotations;

namespace DBRuns.Models
{

    public class SignData
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string NewPassword { get; set; }
    }

}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBRuns.Models
{

    public struct Roles
    {
        public const string ADMIN = "ADMIN";
        public const string MANAGER = "MANAGER";
        public const string USER = "USER";
    }




    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PwdHash { get; set; }
        public string Role { get; set; }
        public bool IsVerified { get; set; }
        public int SignInFailCount { get; set; }


        [NotMapped]
        public string Password 
        {
            set 
            {
                PwdHash = Utils.GetMd5Hash(value);
            }
        }

        public bool IsBlocked = false;
    }

}

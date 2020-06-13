using System;

namespace DBRuns.Models
{

    public struct Roles
    {
        public const string ADMIN = "ADMIN";
        public const string MANAGER = "MANAGER";
        public const string USER = "USER";
    }




    public class SignUpData
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }




    public class User : SignUpData
    {
        public Guid Id { get; set; }
        public string Role { get; set; }
        public bool IsVerified { get; set; }
        public int loginFail { get; set; }
    }

}

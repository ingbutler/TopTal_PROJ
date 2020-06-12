using System;

namespace DBRuns.Models
{

    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsVerified { get; set; }
        public int loginFail { get; set; }
    }

}

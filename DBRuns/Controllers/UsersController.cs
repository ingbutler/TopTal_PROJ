using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using DBRuns.Models;
using DBRuns.Services;

namespace DBRuns.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private UserService UserService { get; }



        public UsersController(UserService userService)
        {
            UserService = userService;
        }



        // GET: api/Users
        [HttpGet]
        public async Task<IEnumerable<User>> GetUser()
        {
            return await UserService.GetUserAsync();
        }



        // GET: api/Users/VerifyUser/00000000-0000-0000-0000-000000000000
        [HttpGet("[action]/{id}")]
        public async Task VerifyUser(Guid id)
        {
            await UserService.VerifyUserAsync(id);
        }



        // POST: api/Users
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {

            // MAY BE REPEATED TO RESEND MAIL
            User exUser = await UserService.GetUserByEmailAsync(user.Email);
            if (exUser == null)
            {
                int result = await UserService.InsertUserAsync(user);
                if (result == 0)
                    return Conflict("User already existing");
            }
            else if(exUser.IsVerified)
                return Conflict("User already existing");

            Task task = UserService.SendVerificationMailAsync(user);

            if (exUser == null)
                return Ok("Check for verification mail");
            else
                return Ok("Verification mail sent again. Check mail"); 
        }

    }

}

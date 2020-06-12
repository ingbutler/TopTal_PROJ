using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Infrastructure;
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
        //private readonly IActionContextAccessor ActionContextAccessor;


        //public UsersController(UserService userService, IActionContextAccessor actionContextAccessor)
        public UsersController(UserService userService)
        {
            UserService = userService;
            //ActionContextAccessor = actionContextAccessor;
        }



        // GET: api/Users
        [HttpGet]
        public async Task<IEnumerable<User>> GetUser()
        {
            return await UserService.GetUserAsync();
        }



        // GET: api/TodoItems/5
        [HttpGet("[action]/{id}")]
        public async Task VerifyUser(Guid id)
        {
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

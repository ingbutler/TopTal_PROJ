using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using DBRuns.Models;
using DBRuns.Services;
using Microsoft.Data.SqlClient;

namespace DBRuns.Controllers
{

    [Authorize(Roles = (Roles.ADMIN + "," + Roles.MANAGER))]
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
        public async Task<ActionResult<ItemList<User>>> GetUser([FromQuery(Name = "filter")] string filter, [FromQuery(Name = "itemsPerPage")] int itemsPerPage, [FromQuery(Name = "pageNumber")] int pageNumber)
        {
            ItemList<User> itemList = null;
            try
            {
                itemList = await UserService.GetUserAsync(filter, itemsPerPage, pageNumber);
            }
            catch(SqlException ex)
            {
                return BadRequest("Check filter condition");
            }

            return itemList;
        }



        // GET: api/Users
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            User user = await UserService.GetUserAsync(id);
            if (user == null)
                return NotFound("No user with such id");
            else
                return user;
        }



        // GET: api/Users
        [HttpGet("[action]")]
        public async Task<ActionResult<User>> GetUserByEmail([FromQuery(Name = "Email")] string email)
        {
            User user = await UserService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound("No user with such EMail");
            else
                return user;
        }



        // GET: api/Users/VerifyUser/00000000-0000-0000-0000-000000000000
        [HttpGet("[action]/{id}")]
        [AllowAnonymous]
        public async Task<ContentResult> VerifyUser(Guid id)
        {
            int result = await UserService.VerifyUserAsync(id);
            if(result == 1)
                return  base.Content("<div>Well done! Your account has been confirmed</div>", "text/html");
            else
                return base.Content("<div>Sorry, user not found</div>", "text/html");
        }



        // POST: api/Users
        [HttpPost]
        public async Task<IActionResult> PostUser(User user)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            int result = await UserService.InsertUserAsync(user);
            if (result == 1)
                return Ok();
            else if (result == -1)
                return Conflict("Account already exists");
            else
                return Problem("User creation was not possible");
        }



        // POST: api/Users/SignUp
        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp(SignData signData)
        {
            User user = await UserService.SignupAsync(signData);
            if (user == null)
                return Problem("No user was created");
            else if (user.Email == null)
                return Conflict("Account already exists");

            Task task = UserService.SendVerificationMailAsync(user);

            return Ok("Check for verification mail");
        }



        // POST: api/Users/SignIn
        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn(SignData signData)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            User user = await UserService.CheckCredentials(signData.Email, signData.Password);
            if (user == null)
                return Unauthorized("Invalid user or password");
            else if(user.IsBlocked)
                return Unauthorized("Account is blocked after too many failed attempts. Ask a manager to unblock it");

            if (!user.IsVerified)
            {
                Task task = UserService.SendVerificationMailAsync(user);
                return Unauthorized("User not verified. Verification mail sent again. Check mail");
            }
            else
            {
                var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
                HttpContext.User = new ClaimsPrincipal(identity);
                return Ok();
            }
        }



        // POST: api/Users/SignIn
        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword(SignData signData)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            User user = await UserService.CheckCredentials(signData.Email, signData.Password);
            if (user == null)
                return Unauthorized("Invalid user or password");
            else if (user.IsBlocked)
                return Unauthorized("Account is blocked after too many failed attempts. Ask a manager to unblock it");

            if (!user.IsVerified)
                return Unauthorized("User not verified. Request sign-in to get verification mail sent again");
            else
            {
                user.Password = signData.NewPassword;
                int result = await UserService.UpdateUserAsync(user);
                if(result == 1)
                    return Ok("Password has been changed");
                else
                    return Problem("Password could not be changed");
            }
        }



        // PUT: api/TodoItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, User user)
        {
            if (id != user.Id)
                return BadRequest();

            int result = await UserService.UpdateUserAsync(user);
            if (result == 1)
                return Ok();
            else if (result == -1)
                return Conflict("EMail belonging to other account");
            else
                return Problem("User could not be updated");
        }



        // DELETE: api/Runs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            int result = await UserService.DeleteUserAsync(id);
            if (result == 1)
                return Ok();
            else if (result == -1)
                return NotFound();
            else
                return Problem("No user deleted");
        }

    }

}

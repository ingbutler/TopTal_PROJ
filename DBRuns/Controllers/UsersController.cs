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
        public async Task<ItemList<User>> GetUser([FromQuery(Name = "filter")] string filter, [FromQuery(Name = "itemsPerPage")] int itemsPerPage, [FromQuery(Name = "pageNumber")] int pageNumber)
        {
            return await UserService.GetUserAsync(filter, itemsPerPage, pageNumber);
        }



        // GET: api/Users
        [HttpGet("{id}")]
        public async Task<User> GetUser(Guid id)
        {
            return await UserService.GetUserAsync(id);
        }



        // GET: api/Users
        [HttpGet("[action]")]
        public async Task<User> GetUserByEmail([FromQuery(Name = "Email")] string email)
        {
            return await UserService.GetUserByEmailAsync(email);
        }



        // GET: api/Users/VerifyUser/00000000-0000-0000-0000-000000000000
        [HttpGet("[action]/{id}")]
        [AllowAnonymous]
        public async Task VerifyUser(Guid id)
        {
            await UserService.VerifyUserAsync(id);
        }



        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            await UserService.InsertUserAsync(user);

            return NoContent();
        }



        // POST: api/Users/SignUp
        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> SignUp(SignData signData)
        {
            User user;
            bool userExists = false;

            user = await UserService.GetUserByEmailAsync(signData.Email);
            if (user == null)
                user = await UserService.SignupAsync(signData);
            else
                return Conflict("Account already exists");

            Task task = UserService.SendVerificationMailAsync(user);

            if (userExists)
                return Ok("Verification mail sent again. Check mail");
            else
                return Ok("Check for verification mail");
        }



        // POST: api/Users/SignIn
        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> SignIn(SignData signData)
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
                return Unauthorized("User not verified. Verification mail sent again.Check mail");
            }
            else
            {
                var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
                HttpContext.User = new ClaimsPrincipal(identity);
                return NoContent();
            }
        }



        // POST: api/Users/SignIn
        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> ChangePassword(SignData signData)
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
                await UserService.UpdateUserAsync(user);
                return Ok("Password has been changed");
            }
        }



        // PUT: api/TodoItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, User user)
        {
            if (id != user.Id)
                return BadRequest();

            int result = await UserService.UpdateUserAsync(user);

            return NoContent();
        }



        // DELETE: api/Runs/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(Guid id)
        {
            User user = await UserService.DeleteUserAsync(id);
            if (user == null)
                return NotFound();
            else
                return user;
        }

    }

}

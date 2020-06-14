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

    [Authorize]
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
        [AllowAnonymous]
        public async Task VerifyUser(Guid id)
        {
            await UserService.VerifyUserAsync(id);
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
                return Unauthorized();

            if (!user.IsVerified)
            {
                Task task = UserService.SendVerificationMailAsync(user);
                return Unauthorized("User not verified. Verification mail sent again.Check mail");
            }
            else
            {
                var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
                HttpContext.User = new ClaimsPrincipal(identity);
                return NoContent();
            }
        }

    }

}

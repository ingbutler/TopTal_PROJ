using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DBRuns.Models;
using DBRuns.Services;
using System.Security.Claims;
using System;

namespace DBRuns.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RunsController : ControllerBase
    {

        public RunService RunService { get; }



        public RunsController(RunService runService)
        {
            RunService = runService;
        }



        // GET: api/Runs
        [HttpGet]
        [Authorize(Roles = Roles.ADMIN + "," + Roles.USER)]
        public async Task<IEnumerable<Run>> GetRun()
        {
            Guid? userId = null;
            if (Utils.GetUserRole(this.User) != Roles.ADMIN)
                userId = Utils.GetUserId(this.User);

            return await RunService.GetRunAsync(userId);
        }



        // POST: api/Runs
        [HttpPost]
        public async Task<ActionResult<Run>> PostRun(RunInput runInput)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            await RunService.InsertRunAsync(
                    Utils.GetUserId(this.User),
                    runInput
                );
            
            return NoContent();
        }

    }

}

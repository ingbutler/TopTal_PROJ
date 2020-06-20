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
        public async Task<ItemList<Run>> GetRun([FromQuery(Name = "filter")] string filter, [FromQuery(Name = "itemsPerPage")] int itemsPerPage, [FromQuery(Name = "pageNumber")] int pageNumber)
        {
            Guid? userId = null;
            if (Utils.GetUserRole(this.User) != Roles.ADMIN)
                userId = Utils.GetUserId(this.User);

            return await RunService.GetRunAsync(userId, filter, itemsPerPage, pageNumber);
        }



        // GET: api/Runs
        [HttpGet("[action]")]
        public async Task<ItemList<ReportItem>> GetReport([FromQuery(Name = "year")] int year, [FromQuery(Name = "itemsPerPage")] int itemsPerPage, [FromQuery(Name = "pageNumber")] int pageNumber, [FromQuery(Name = "userId")] Guid userId)
        {
            if (Utils.GetUserRole(this.User) != Roles.ADMIN)
                userId = Utils.GetUserId(this.User);

            return await RunService.GetReportAsync(userId, year, itemsPerPage, pageNumber);
        }



        // POST: api/Runs
        [HttpPost]
        public async Task<ActionResult<Run>> PostRun([FromQuery(Name = "userId")] Guid userId, RunInput runInput)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (Utils.GetUserRole(this.User) != Roles.ADMIN)
                userId = Utils.GetUserId(this.User);

            await RunService.InsertRunAsync(
                    userId,
                    runInput
                );
            
            return NoContent();
        }



        // PUT: api/TodoItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRun(Guid id, Run run)
        {
            if (id != run.Id)
                return BadRequest();

            if (Utils.GetUserRole(this.User) != Roles.ADMIN)
            {
                Guid userId = Utils.GetUserId(this.User);
                if (run.UserId != userId)
                    return Unauthorized();
            }

            int result = await RunService.UpdateRunAsync(run);

            return NoContent();
        }



        // DELETE: api/Runs/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Run>> DeleteRun(Guid id)
        {
            Run run = await RunService.GetRunAsync(id);

            if (Utils.GetUserRole(this.User) != Roles.ADMIN)
            {
                Guid userId = Utils.GetUserId(this.User);
                if (run.UserId != userId)
                    return Unauthorized();
            }

            int result = await RunService.DeleteRunAsync(run);
            if (result == 0)
                return NotFound();
            else
                return run;
        }



        // DELETE: api/Runs/5
        [HttpDelete("[action]/{userId}")]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<ActionResult<Run>> DeleteByUser(Guid userId)
        {
            await RunService.DeleteRunByUserAsync(userId);
            return NoContent();
        }

    }

}

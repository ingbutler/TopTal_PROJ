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
        public async Task<ItemList<ReportItem>> GetReport([FromQuery(Name = "year")] int year, [FromQuery(Name = "itemsPerPage")] int itemsPerPage, [FromQuery(Name = "pageNumber")] int pageNumber)
        {
            Guid userId = Utils.GetUserId(this.User);
            return await RunService.GetReportAsync(userId, year, itemsPerPage, pageNumber);
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



        // POST: api/Runs
        [HttpPost("{userId}")]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<ActionResult<Run>> PostRun(Guid userId, RunInput runInput)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            await RunService.InsertRunAsync(
                    userId,
                    runInput
                );

            return NoContent();
        }



        // DELETE: api/Runs/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Run>> DeleteRun(Guid id)
        {
            Run run = await RunService.DeleteRunAsync(id);
            if (run == null)
                return NotFound();
            else
                return run;
        }



        // DELETE: api/Runs/5
        [HttpDelete("{userId}")]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<ActionResult<Run>> DeleteByUser(Guid userId)
        {
            await RunService.DeleteRunAsync(userId);
            return NoContent();
        }

    }

}

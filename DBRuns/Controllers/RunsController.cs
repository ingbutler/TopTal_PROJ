using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DBRuns.Models;
using DBRuns.Services;
using System.Security.Claims;
using System;
using Microsoft.Data.SqlClient;

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
        public async Task<ActionResult<ItemList<Run>>> GetRun([FromQuery(Name = "filter")] string filter, [FromQuery(Name = "itemsPerPage")] int itemsPerPage, [FromQuery(Name = "pageNumber")] int pageNumber)
        {
            Guid? userId = null;
            if (Utils.GetUserRole(this.User) != Roles.ADMIN)
                userId = Utils.GetUserId(this.User);

            ItemList<Run> itemList = null;
            try
            {
                itemList = await RunService.GetRunAsync(userId, filter, itemsPerPage, pageNumber);
            }
            catch(SqlException ex)
            {
                return BadRequest("Check filter condition");
            }

            return itemList;
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
        public async Task<IActionResult> PostRun([FromQuery(Name = "userId")] Guid userId, RunInput runInput)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (Utils.GetUserRole(this.User) != Roles.ADMIN)
                userId = Utils.GetUserId(this.User);

            int result = await RunService.InsertRunAsync(
                    userId,
                    runInput
                );
            if (result == 1)
                return Ok();
            else
                return Problem("Record creation was not possible");
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
            if (result == 1)
                return Ok();
            else
                return Problem("The record could not be updated");
        }



        // DELETE: api/Runs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRun(Guid id)
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
                return NotFound("No record could be deleted");
            else
                return Ok();
        }



        // DELETE: api/Runs/5
        [HttpDelete("[action]/{userId}")]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<IActionResult> DeleteByUser(Guid userId)
        {
            int result = await RunService.DeleteRunByUserAsync(userId);
            if(result == 0)
                return NotFound("No record was deleted");
            else
                return Ok();
        }

    }

}

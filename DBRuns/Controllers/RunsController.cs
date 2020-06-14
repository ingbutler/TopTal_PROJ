using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DBRuns.Models;
using DBRuns.Services;

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
        public async Task<IEnumerable<Run>> GetRun()
        {
            return await RunService.GetRunAsync();
        }



        // POST: api/Runs
        [HttpPost]
        public async Task<ActionResult<Run>> PostRun(RunInput runInput)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            await RunService.InsertRunAsync(runInput);

            return NoContent();
        }

    }

}

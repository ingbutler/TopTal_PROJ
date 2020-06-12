using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DBRuns.Models;
using DBRuns.Services;

namespace DBRuns.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
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

    }

}

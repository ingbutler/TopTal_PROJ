using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using DBRuns.Data;
using DBRuns.Models;
using Microsoft.AspNetCore.Mvc;

namespace DBRuns.Services
{

    public class RunService
    {

        private readonly DBRunContext Context;



        public RunService(DBRunContext context)
        {
            Context = context;
        }



        public async Task<IEnumerable<Run>> GetRunAsync()
        {
            return await Context.Runs.ToListAsync();
        }



        public async Task<int> InsertRunAsync(Run run)
        {
            run.Id = Guid.NewGuid();
            run.Date = DateTime.Today;

            if (run.UserId == null)
                throw new ArgumentNullException("User not specified");

            if (run.Distance < 0)
                throw new ArgumentOutOfRangeException("Distance less than zero");

            if (run.TimeRun < 0)
                throw new ArgumentOutOfRangeException("Time less than zero");

            Context.Runs.Add(run);
            return await Context.SaveChangesAsync();
        }



        public async Task<int> UpdateRunAsync(Guid id, Run run)
        {
            if (id != run.Id)
                throw new ArgumentException("Id not corresponding");

            Context.Entry(run).State = EntityState.Modified;

            //try
            //{
            //    await Context.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!RunExists(id))
            //    {
            //        throw new DbUpdateConcurrencyException("Item not updated");
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}

            return await Context.SaveChangesAsync();
        }



        public async Task<Run> DeleteRunAsync(Guid id)
        {
            var run = await Context.Runs.FindAsync(id);
            if (run == null)
                return null;

            Context.Runs.Remove(run);
            await Context.SaveChangesAsync();

            return run;
        }



        private bool RunExists(Guid id)
        {
            return Context.Runs.Any(e => e.Id == id);
        }


    }

}

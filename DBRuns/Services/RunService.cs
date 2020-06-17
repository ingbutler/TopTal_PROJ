using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using DBRuns.Data;
using DBRuns.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DBRuns.Services
{

    public class RunService
    {

        private readonly DBRunContext Context;
        private readonly IConfiguration Configuration;



        public RunService(DBRunContext context, IConfiguration configuration)
        {
            Context = context;
            Configuration= configuration;
        }



        public async Task<IEnumerable<Run>> GetRunAsync(Guid? userId)
        {
            IQueryable<Run> runs = Context.Runs.FromSqlInterpolated($"select * from Runs");

            if (userId != null)
                runs = runs.Where(x => x.UserId == userId);

            return await runs.ToListAsync();
        }



        public async Task<int> InsertRunAsync(Guid userId, RunInput runInput)
        {
            if (runInput.Distance <= 0)
                throw new ArgumentOutOfRangeException("Distance must be greater than zero");

            if (runInput.TimeRun <= 0)
                throw new ArgumentOutOfRangeException("Time must be greater than zero");


            string weather = "";
            string weatherReqUri = Configuration["WeatherByCityReq"];
            weatherReqUri = weatherReqUri.Replace("{cityCountry}", runInput.Location);

            using (HttpClient httpClient = new HttpClient())
            {
                weather = await httpClient.GetStringAsync(weatherReqUri);
            }

            if (weather.Length > 1000)          // Limitazione alla lunghezza della colonna nel db
                weather = weather.Substring(0, 1000);

            Run run =
                new Run()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Date = runInput.Date,
                    Distance = runInput.Distance,
                    TimeRun = runInput.TimeRun,
                    Location = runInput.Location,
                    Weather = weather
                };

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



        public async Task<IEnumerable<ReportItem>> GetReportAsync(Guid userId)
        {
            string sql =
                @"
                    select
                        tt.WeekStart
                       ,tt.WeekNumber
                       ,tt.Year
                       ,tt.TotalTime
                       ,tt.TotalDistance
                       ,(tt.TotalDistance / tt.TotalTime) as AverageSpeed
                    from
	                    (
                            select
                                t.WeekStart
                               ,t.WeekNumber
                               ,t.Year
                               ,sum(t.TimeRun) as TotalTime
                               ,sum(t.Distance) as TotalDistance
                            from
	                            (
		                            select
		                                dateadd(dd, -(datepart(dw, Date) - 1), convert(date, Date)) as WeekStart
		                               ,datepart(week, Date) as WeekNumber
                                       ,year(Date) as Year
                                       ,Distance
                                       ,TimeRun
		                            from
			                            Runs
                                    where
                                        UserId = {0}
	                            ) as t
                            group by
	                            t.WeekStart
                               ,t.WeekNumber
                               ,t.Year
                        ) as tt
                    order by
	                    tt.WeekStart
                ";

            IQueryable<ReportItem> reportItems = 
                Context.ReportItems.FromSqlRaw(sql, new object[] { userId });

            return await reportItems.ToListAsync();
        }


    }

}

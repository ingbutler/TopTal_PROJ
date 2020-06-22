using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DBRuns.Data;
using DBRuns.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

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



        public async Task<Run> GetRunAsync(Guid id)
        {
            return await Context.Runs.FirstOrDefaultAsync(x => x.Id == id);
        }



        public async Task<ItemList<Run>> GetRunAsync(Guid? userId, string filter, int itemsPerPage, int pageNumber)
        {
            if (itemsPerPage == 0)
                itemsPerPage = Int32.Parse(Configuration["ItemsPerPageDefault"]);
            if (pageNumber == 0)
                pageNumber = Int32.Parse(Configuration["PageNumberDefault"]);

            ItemList<Run> itemList =
                new ItemList<Run>()
                {
                    ItemsPerPage = itemsPerPage,
                    PageNumber = pageNumber
                };

            int offset = itemsPerPage * (pageNumber - 1);
            String sql;
            string whereStr = "";
            List<string> parms;
            string parsedFilter = Utils.ParseFilter(filter, typeof(Run), out parms);

            if (!string.IsNullOrEmpty(parsedFilter))
                whereStr = " where " + parsedFilter;

            if (userId != null)
            {
                if(whereStr == "")
                    whereStr += " where UserId = '" + userId + "'";
                else
                    whereStr += " and UserId = '" + userId + "'";
            }

            sql =
                @"
                    select 
                        count(*) as count
                    from 
                        Runs
                    " + whereStr;
            List<ItemsCount> itemsCounts = await Context.ItemsCounts.FromSqlRaw(sql, parms.ToArray()).ToListAsync();
            itemList.QueriedItemsCount = itemsCounts.First().Count;
            itemList.PageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(itemList.QueriedItemsCount) / Convert.ToDecimal(itemsPerPage)));

            sql =
                $@"
                    select 
                        * 
                    from 
                        Runs 
                    " + whereStr + @"
                    order by 
                        Date
                    offset
                        " + offset + @" rows
                    fetch next
                        " + itemsPerPage + @" rows only
                ";
            itemList.items = await Context.Runs.FromSqlRaw(sql, parms.ToArray()).ToListAsync();

            return itemList;
        }



        public async Task<int> InsertRunAsync(Guid userId, RunInput runInput)
        {
            if (runInput.Distance <= 0)
                throw new ArgumentOutOfRangeException("Distance must be greater than zero");

            if (runInput.Time <= 0)
                throw new ArgumentOutOfRangeException("Time must be greater than zero");


            DateTime dd = runInput.Date.AddDays(-1);
            string dt = new DateTimeOffset(dd, TimeSpan.Zero).ToUnixTimeSeconds().ToString();


            string weather = "";
            string weatherReqUri;
            HttpResponseMessage response;

            using (HttpClient httpClient = new HttpClient())
            {
                weatherReqUri = Configuration["WeatherByCityReq"];
                weatherReqUri = weatherReqUri.Replace("{cityCountry}", runInput.Location);
                response = await httpClient.GetAsync(weatherReqUri);
                string s = await response.Content.ReadAsStringAsync();

                s = s.Substring(16);
                int cp = s.IndexOf(',');
                string lon = s.Substring(0, cp);
                s = s.Substring(lon.Length + 7);
                cp = s.IndexOf('}');
                string lat = s.Substring(0, cp);

                weatherReqUri = Configuration["HistoricalWeatherReq"];
                weatherReqUri =
                    weatherReqUri
                        .Replace("{lat}", lat)
                        .Replace("{lon}", lon)
                        .Replace("{time}", dt);
                response = await httpClient.GetAsync(weatherReqUri);
                weather = await response.Content.ReadAsStringAsync();
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
                    Time = runInput.Time,
                    Location = runInput.Location,
                    Weather = weather
                };

            Context.Runs.Add(run);
            return await Context.SaveChangesAsync();
        }



        public async Task<int> UpdateRunAsync(Run run)
        {
            Context.Entry(run).State = EntityState.Modified;

            return await Context.SaveChangesAsync();
        }



        public async Task<int> DeleteRunAsync(Run run)
        {
            Context.Runs.Remove(run);
            return await Context.SaveChangesAsync();
        }



        public async Task<int> DeleteRunByUserAsync(Guid userId)
        {
            string sql = "delete from Runs where UserId = {0}";
            return await Context.Database.ExecuteSqlRawAsync(sql, userId);
        }



        private bool RunExists(Guid id)
        {
            return Context.Runs.Any(e => e.Id == id);
        }



        public async Task<ItemList<ReportItem>> GetReportAsync(Guid userId, int year, int itemsPerPage, int pageNumber)
        {
            if (itemsPerPage == 0)
                itemsPerPage = Int32.Parse(Configuration["ItemsPerPageDefault"]);
            if (pageNumber == 0)
                pageNumber = Int32.Parse(Configuration["PageNumberDefault"]);

            ItemList<ReportItem> itemList =
                new ItemList<ReportItem>()
                {
                    ItemsPerPage = itemsPerPage,
                    PageNumber = pageNumber
                };

            int offset = itemsPerPage * (pageNumber - 1);
            object[] parms = new object[] { userId, year };


            string sql =
                @"
                    select 
                        count(distinct (year(Date) + datepart(week, Date))) as count
                    from
                        Runs
                    where
                        UserId = {0}
                        and year(Date) = {1}
                ";
            List<ItemsCount> itemsCounts = await Context.ItemsCounts.FromSqlRaw(sql, parms).ToListAsync();
            itemList.QueriedItemsCount = itemsCounts.First().Count;
            itemList.PageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(itemList.QueriedItemsCount) / Convert.ToDecimal(itemsPerPage)));

            sql =
                @"
                    select
                        tt.WeekStart
                       ,tt.WeekNumber
                       ,tt.Year
                       ,tt.RunCount
                       ,tt.TotalTime
                       ,tt.TotalDistance
                       ,(tt.TotalDistance / tt.TotalTime) as AverageSpeed
                    from
	                    (
                            select
                                t.WeekStart
                               ,t.WeekNumber
                               ,t.Year
                               ,count(*) as RunCount
                               ,sum(t.Time) as TotalTime
                               ,sum(t.Distance) as TotalDistance
                            from
	                            (
		                            select
		                                dateadd(dd, -(datepart(dw, Date) - 1), convert(date, Date)) as WeekStart
		                               ,datepart(week, Date) as WeekNumber
                                       ,year(Date) as Year
                                       ,Distance
                                       ,Time
		                            from
			                            Runs
                                    where
                                        UserId = {0}
                                        and year(Date) = {1}
	                            ) as t
                            group by
	                            t.WeekStart
                               ,t.WeekNumber
                               ,t.Year
                        ) as tt
                    order by
	                    tt.WeekStart
                    offset
                        " + offset + @" rows
                    fetch next
                        " + itemsPerPage + @" rows only
                ";

            itemList.items = await Context.ReportItems.FromSqlRaw(sql, parms).ToListAsync();

            return itemList;
        }

    }

}

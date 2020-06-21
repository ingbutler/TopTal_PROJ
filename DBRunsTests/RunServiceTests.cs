using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DBRuns.Data;
using DBRuns.Models;
using DBRuns.Services;
using DBRuns.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace DBRunsTests
{

    [TestClass]
    public class RunServiceTests
    {

        [TestMethod]
        public async Task InsertOk()
        {

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("AppSettings.json");
            IConfiguration configuration = configurationBuilder.Build();

            RunService srv =
                new RunService(
                    //new DBRunContext(
                    //    new DbContextOptionsBuilder<DBRunContext>().UseSqlServer(
                    //        configuration.GetConnectionString("DefaultConnection")
                    //    ).Options
                    //),
                    new DBRunContext(new DbContextOptionsBuilder<DBRunContext>().UseInMemoryDatabase("DBRuns").Options),
                    configuration
                );



            // Arrange
            int expected = 1;

            RunInput runInput01 =
                new RunInput()
                {
                    Date = new System.DateTime(2020, 6, 20, 19, 51, 0),
                    Distance = 5600,
                    Time = 1100,
                    Location = "Poggibonsi,IT"
                };


            // Act
            int retValue =
                await srv.InsertRunAsync(
                        new Guid("00000000-0000-0000-0000-000000000000"),
                        runInput01
                    );


            // Assert
            int actual = retValue;
            Assert.AreEqual(expected, actual, 0, "Ok, record inserted");
        }



        [DataTestMethod]
        [DataRow(0, 1000)]
        [DataRow(-100, 1000)]
        [DataRow(1000, 0)]
        [DataRow(1000, -100)]
        public async Task InsertDistanceTimeLEZero(int distance, int time)
        {

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("AppSettings.json");
            IConfiguration configuration = configurationBuilder.Build();

            RunService srv =
                new RunService(
                    new DBRunContext(new DbContextOptionsBuilder<DBRunContext>().UseInMemoryDatabase("DBRuns").Options),
                    configuration
                );



            // Arrange
            RunInput runInput01 =
                new RunInput()
                {
                    Date = new System.DateTime(2020, 6, 20, 19, 51, 0),
                    Distance = distance,
                    Time = time,
                    Location = "Poggibonsi,IT"
                };



            // Assert
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
                () =>
                    srv.InsertRunAsync(
                            new Guid("00000000-0000-0000-0000-000000000000"),
                            runInput01
                        )
                );

        }



        [TestMethod]
        public async Task PostRunOk()
        {

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("AppSettings.json");
            IConfiguration configuration = configurationBuilder.Build();

            RunService srv =
                new RunService(
                    //new DBRunContext(
                    //    new DbContextOptionsBuilder<DBRunContext>().UseSqlServer(
                    //        configuration.GetConnectionString("DefaultConnection")
                    //    ).Options
                    //),
                    new DBRunContext(new DbContextOptionsBuilder<DBRunContext>().UseInMemoryDatabase("DBRuns").Options),
                    configuration
                );

            RunsController rc = new RunsController(srv);
            var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000000"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "USER"));

            rc.ControllerContext = new ControllerContext();
            rc.ControllerContext.HttpContext = new DefaultHttpContext();
            rc.HttpContext.User = new ClaimsPrincipal(identity);



            // Arrange
            ActionResult<Run> expected = new NoContentResult();

            RunInput runInput01 =
                new RunInput()
                {
                    Date = new System.DateTime(2020, 6, 20, 19, 51, 0),
                    Distance = 5600,
                    Time = 1100,
                    Location = "Poggibonsi,IT"
                };


            // Act
            ActionResult<Run> retValue =
                await rc.PostRun(
                        new Guid("00000000-0000-0000-0000-000000000000"),
                        runInput01
                    );


            // Assert
            ActionResult<Run> actual = retValue;
            Assert.AreEqual(expected, actual, "Ok, record inserted");

        }




    }

}

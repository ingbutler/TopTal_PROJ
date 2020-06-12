using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DBRuns.Models;
using DBRuns.Data;
using DBRuns.Services;
using System.Threading.Tasks;

namespace DBRunsTests
{

    [TestClass]
    public class RunServiceTests
    {

        [TestMethod]
        public async Task InsertOk()
        {
            RunService srv =
                new RunService(
                    new DBRunContext(
                            new DbContextOptionsBuilder<DBRunContext>().UseInMemoryDatabase("DBRuns").Options
                    )
                );



            // Arrange
            int expected = 2;

            Run run01 =
                new Run()
                {

                };



            // Act
            int retValue = await srv.InsertRunAsync(run01);



            // Assert
            int actual = retValue;
            Assert.AreEqual(expected, actual, 0, "No record inserted");

        }

    }

}

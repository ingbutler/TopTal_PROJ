using System;

namespace DBRuns.Models
{

    public class Run
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal Distance { get; set; }
        public decimal TimeRun { get; set; }
        public string Location { get; set; }
        public string Weather { get; set; }
    }

}

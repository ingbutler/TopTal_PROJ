using System;

namespace DBRuns.Models
{

    public class ReportItem
    {
        public DateTime WeekStart { get; set; }
        public int WeekNumber { get; set; }
        public int Year { get; set; }
        public int RunCount { get; set; }
        public decimal TotalTime { get; set; }
        public decimal TotalDistance { get; set; }
        public decimal AverageSpeed { get; set; }
    }

}

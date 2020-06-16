﻿using System;
using System.ComponentModel.DataAnnotations;

namespace DBRuns.Models
{

    public class RunInput
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int Distance { get; set; }
        [Required]
        public int TimeRun { get; set; }
        [Required]
        public string Location { get; set; }
    }




    public class Run : RunInput
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Weather { get; set; }
    }

}

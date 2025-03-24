using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class Activity
    {
        public int activity_id { get; set; }
        public int activity_order { get; set; }
        public string activity_name { get; set; }
        public string activity_name_long { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class AssaySample
    {
        public int? activity_qty { get; set; }
        public IEnumerable<Activity> activity_list { get; set; }
        public int status_id { get; set; }
        public string status { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public int? type_id { get; set; }
        public string type_name { get; set; }
        public bool is_active { get; set; }
        public string support_code { get; set; }
        public int? support_master_id { get; set; }
        public int? materials_count { get; set; }
    }
    
}

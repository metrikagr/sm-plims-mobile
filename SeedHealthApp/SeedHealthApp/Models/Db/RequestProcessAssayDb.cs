using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models.Db
{
    [Table("RequestProcessAssay")]
    public class RequestProcessAssayDb
    {
        [PrimaryKey]
        public int request_process_assay_id { get; set; }
        public int sample_type_id { get; set; }
        public string sample_type_name { get; set; }
        public string status { get; set; }
        public string status_name { get; set; }
        public bool is_active { get; set; }
        public int? activity_qty { get; set; }
        public int? type_id { get; set; }
        public string type_name { get; set; }
        public string start_date { get; set; }
        public string finish_date { get; set; }
        //public IEnumerable<Activity> activity_list { get; set; }
        public bool is_available_offline { get; set; }
        public DateTime? last_synced_date { get; set; }
    }
}

using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models.Db
{
	[Table("RequestProcessAssayActivitySample")]
	public class RequestProcessAssayActivitySampleDb
    {
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public int request_process_essay_activity_sample_id { get; set; }
		public string local_guid { get; set; }
        public int request_process_essay_id { get; set; }
        //public int? request_id { get; set; }
        //public int? num_order_id { get; set; }
        //public int? assay_id { get; set; } //essay_id
        public int? composite_sample_id { get; set; }
        //public int? composite_sample_type_id { get; set; }
        //public int crop_id { get; set; }
        //public int workflow_id { get; set; }
        public int? num_order { get; set; }
        public int? request_process_essay_activity_sample_status_id { get; set; }
		//public string status_name { get; set; }
		public int? number_of_seeds { get; set; }
        //public int? request_process_essay_activity_id { get; set; }
        public int? activity_id { get; set; }
        //public int? composite_sample_group_id { get; set; }
        public bool MarkedToSync { get; set; }
		public DateTime Modified { get; set; }
    }
}

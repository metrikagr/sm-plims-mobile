using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class AssayActivitySample
    {
        public int request_process_essay_activity_sample_id { get; set; }
        public int request_process_essay_id { get; set; }
        public int activity_id { get; set; }
        public int composite_sample_id { get; set; }
        public string composite_sample_name { get; set; }
        public int? activity_sample_order { get; set; }
        public int? number_of_seeds { get; set; }
        public int LocalId { get; set; }
    }
}

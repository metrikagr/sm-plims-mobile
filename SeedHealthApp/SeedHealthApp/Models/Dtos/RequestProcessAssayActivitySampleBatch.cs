using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models.Dtos
{
    public class RequestProcessAssayActivitySampleBatch
    {
        //public int request_process_essay_activity_sample_id { get; set; }
        //public int request_process_essay_id { get; set; }
        public int activity_id { get; set; }
        public int composite_sample_id { get; set; }
        public int? number_of_seeds { get; set; }
        public int? status_id { get; set; }
    }
}

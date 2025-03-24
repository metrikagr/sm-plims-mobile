using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class Result
    {
        public int request_process_assay_id { get; set; }
        public int activity_id { get; set; }
        public int composite_sample_id { get; set; }
        public int agent_id { get; set; }
        public float? number_result { get; set; }
        public string auxiliary_result { get; set; }
        public string text_result { get; set; }
        public bool reprocess { get; set; }
        //public int? support_order { get; set; }
        //public int? cell_position { get; set; }
        //public int? grafting_number { get; set; }
        //public int? reading_data_type { get; set; }
        //public int? support_master { get; set; }
    }
}

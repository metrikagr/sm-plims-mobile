using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class MultiRequest
    {
        public int event_id { get; set; }
        public string event_code { get; set; }
        public string request_codes { get; set; }
        public string crop_names { get; set; }
        public string distribution_names { get; set; }
        public string sample_types { get; set; }
        public string start_date { get; set; }
        public int status_id { get; set; }
        public string status_name { get; set; }
    }
}

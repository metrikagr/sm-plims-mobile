using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class ElisaEvent
    {
        public int event_id { get; set; }
        public IEnumerable<int> agents { get; set; }
        public IEnumerable<RequestProcess> request_process { get; set; }
        public string event_code { get; set; }
        public int status_id { get; set; }
        public string status_name { get; set; }
    }
}

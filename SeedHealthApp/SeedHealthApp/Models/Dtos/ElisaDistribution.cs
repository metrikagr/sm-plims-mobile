using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class ElisaDistribution
    {
        public IEnumerable<int> agent_ids { get; set; }
        public List<SupportLocation> distributions { get; set; }
    }

    public class SupportLocation
    {
        public int event_id { get; set; }
        public int agent_id { get; set; }
        public int support_order_id { get; set; }
        public int cell_position { get; set; }
        public int support_cell_position { get; set; }
        public int? composite_sample_id { get; set; }
        public int reading_data_type_id { get; set; }
        public int request_id { get; set; }
        public int sample_type_id { get; set; }
        public int request_process_essay_id { get; set; }
        public string composite_sample_name { get; set; }
        public bool to_delete { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class SampleMaterial
    {
        public int composite_sample_id { get; set; }
        public string composite_sample_name { get; set; }
        public int? num_order { get; set; }
        public int? tested_seeds_count { get; set; }
    }
}

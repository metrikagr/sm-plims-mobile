using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class Assay
    {
        public int assay_id { get; set; }
        public string assay_name { get; set; }
        public string crop_name { get; set; }
        public bool is_active { get; set; }
        public int process_id { get; set; }
        public int request_id { get; set; }
        public string source_name { get; set; }
        public string status { get; set; }
        public string code { get; set; }
    }
    /*
     crop: "Small grain cereals"
deleted: "active"
name: "Blotter and freeze paper test"
num_order: "3"
sample: "composite sample header"
source: "mobile app source"
state: "pending"
    */
}

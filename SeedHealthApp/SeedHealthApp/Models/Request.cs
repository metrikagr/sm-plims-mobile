using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class Request
    {
        public int crop_id { get; set; }
        public string crop_name { get; set; }
        public int distribution_id { get; set; }
        public string distribution_name { get; set; }
        public string updated_date { get; set; }
        public string request_code { get; set; }
        public int request_id { get; set; }
        public int sample_group_id { get; set; }
        public string registered_date { get; set; }
        public string status_name { get; set; }

        /*
        crop_id: "4"
        crop_name: "Small grain cereals"
        distribution_id: "11"
        distribution_name: "shipment"
        updated_date: "2021-07-22 21:53:34"
        request_code: "SH21-304"
        request_id: "5"
         */
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SeedHealthApp.Models
{
    public class RequestLookup
    {
        public int request_id { get; set; }
        public int process_id { get; set; }
        public string request_code { get; set; }
        public string formatted_request_code => $"{request_code}{(process_id > 1 ? " (R)": string.Empty)}";
    }
}

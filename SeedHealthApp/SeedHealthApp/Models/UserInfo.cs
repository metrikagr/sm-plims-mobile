using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class UserInfo
    {
        public int user_id { get; set; }
        public string username { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public int? role_id { get; set; }
        public string role_name { get; set; }
        public int? institution_id { get; set; }
        public string institution_name { get; set; }
        public int? laboratory_id { get; set; }
        public string laboratory_name { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class BreadcumItem
    {
        public int Order { get; set; }
        public string Title { get; set; }
        public string NavigationUri { get; set; }
        public bool IsFirst { get; set; }
    }
}

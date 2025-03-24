using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class ApiResponse<T>
    {
        //public bool Success => !MessageList.Any();
        //public List<string> MessageList { get; private set; }
        public int Code { get; set; }
        public int Status { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
    }
}

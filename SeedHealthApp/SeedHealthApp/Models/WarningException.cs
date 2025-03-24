using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class WarningException : Exception
    {
        public WarningException(string message) : base(message)
        {

        }
    }
}

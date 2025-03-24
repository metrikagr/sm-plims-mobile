using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models.Dtos
{
    public class BlotterStepsDto
    {
        public RequestProcessAssayStep1Dto preparation { get; set; }
        public RequestProcessAssayStep1Dto evaluation { get; set; }
    }
}

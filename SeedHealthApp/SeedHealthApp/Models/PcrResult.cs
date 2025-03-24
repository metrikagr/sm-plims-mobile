using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class PcrResult
    {
        //public SampleMaterial Material { get; set; }
        public int MaterialId { get; set; }
        public IEnumerable<PcrAgentResult> AgentResults { get; set; }
        public int ActivityId { get; set; }
    }
}

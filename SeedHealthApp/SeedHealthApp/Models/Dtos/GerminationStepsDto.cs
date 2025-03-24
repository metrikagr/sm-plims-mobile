using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models.Dtos
{
    public class GerminationStepsDto
    {
        public RequestProcessAssayStep1Dto preparation { get; set; }
        public RequestProcessAssayStep2Dto evaluation { get; set; }
        public RequestProcessAssayStep3Dto evaluation_symptom { get; set; }
    }

    public class RequestProcessAssayStep1Dto
    {
        public int? acum { get; set; }
        public int? total_act_spl { get; set; }
        public int? total_acum { get; set; }
        public IList<string> progress { get; set; }
        public string users { get; set; }
        public float percent { get; set; }
        public string status { get; set; }
        public IEnumerable<string> start_date { get; set; }
        public IEnumerable<string> finish_date { get; set; }
    }
    public class RequestProcessAssayStep2Dto
    {
        public int? acum { get; set; }
        public int? total_act_spl { get; set; }
        public int? total_acum { get; set; }
        public IList<string> progress { get; set; }
        public string users { get; set; }
        public float percent { get; set; }
        public string status { get; set; }
        public IEnumerable<string> start_date_1 { get; set; }
        public IEnumerable<string> finish_date_1 { get; set; }
    }
    public class RequestProcessAssayStep3Dto
    {
        public int? acum { get; set; }
        public int? total_act_spl { get; set; }
        public int? total_acum { get; set; }
        public IList<string> progress { get; set; }
        public string users { get; set; }
        public float percent { get; set; }
        public string status { get; set; }
        public IEnumerable<string> start_date_2 { get; set; }
        public IEnumerable<string> finish_date_2 { get; set; }
    }
}

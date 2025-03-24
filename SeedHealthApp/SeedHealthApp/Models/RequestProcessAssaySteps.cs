using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeedHealthApp.Models
{
    public class RequestProcessAssaySteps
    {
        public RequestProcessAssayStep preparation { get; set; }
        public RequestProcessAssayStep evaluation { get; set; }

        public RequestProcessAssayStep sub_process_3 { get; set; }
    }

    public class RequestProcessAssayStep
    {
        public int? acum { get; set; }
        public int? total_act { get; set; }
        public int? total_acum { get; set; }
        public IList<string> progress { get; set; }
        public string users { get; set; }
        public float percent { get; set; }
        public string StepProgress
        {
            get
            {
                if (progress == null)
                    return string.Empty;
                var tempProgress = new string[progress.Count()];
                for (int i = 0; i < progress.Count(); i++)
                {
                    tempProgress[i] = $"Rep {i + 1}: {progress[i]}";
                }
                return string.Join(", ", tempProgress);
            }
        }
        public string status { get; set; }
        public string start_date { get; set; } = string.Empty;
        public string finish_date { get; set; } = string.Empty;

    }
}

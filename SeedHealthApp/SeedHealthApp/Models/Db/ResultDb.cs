using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models.Db
{
    [Table("Result")]
    public class ResultDb
    {
        [PrimaryKey, AutoIncrement]
        public int reading_data_id { get; set; }
        public int request_process_assay_id { get; set; }
        public int activity_id { get; set; }
        public int composite_sample_id { get; set; }
        public int agent_id { get; set; }
        public float? number_result { get; set; }
        public string auxiliary_result { get; set; }
        public string text_result { get; set; }

        public bool MarkedToSync { get; set; }
        public DateTime Modified { get; set; }
        public bool reprocess { get; set; }
    }
}

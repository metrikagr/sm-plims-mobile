using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class SampleType : Prism.Mvvm.BindableBase
    {
        public int sample_type_id { get; set; }
        public string sample_type_name { get; set; }
        public string status { get; set; }
        public bool is_active { get; set; }
        public string status_name { get; set; }
        public int? activity_qty { get; set; }
        public int? type_id { get; set; }
        public string type_name { get; set; }
        public string start_date { get; set; }
        public string finish_date { get; set; }
        public IEnumerable<Activity> activity_list { get; set; }
        public int request_process_essay_id { get; set; }
        private bool _isAvailableOffline;
        public bool IsAvailableOffline
        {
            get { return _isAvailableOffline; }
            set { SetProperty(ref _isAvailableOffline, value); }
        }
        private DateTime? _lastSyncedDate;
        public DateTime? LastSyncedDate
        {
            get { return _lastSyncedDate; }
            set { SetProperty(ref _lastSyncedDate, value); }
        }
        public int assay_id { get; set; }
        public int process_id { get; set; }
        public string code { get; set; }
        public string request_code { get; set; }
        public int request_id { get; set; }
        public string RequestCodeSampleType { get => $"{request_code}{(process_id>1?" (R)":"")} {sample_type_name}"; }
    }
}

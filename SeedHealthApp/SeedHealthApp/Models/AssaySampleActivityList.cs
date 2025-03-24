using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class AssaySampleActivityList
    {
        public int composite_sample_id { get; set; }
        public string composite_sample_name { get; set; }
        public int? num_order { get; set; }
        public IEnumerable<AssaySampleActivityCell> tested_seeds_counts { get; set; }

        public AssaySampleActivityList()
        {
            tested_seeds_counts = new AssaySampleActivityCell[10];
        }
    }

    public class AssaySampleActivityCell : Prism.Mvvm.BindableBase
    {
        public int request_process_essay_activity_sample_id { get; set; }
        public int composite_sample_id { get; set; }
        public int activity_id { get; set; }
        public bool Enabled { get; set; }
        private int? _value;
        public int? Value
        {
            get { return _value; }
            set
            {
                SetProperty(ref _value, value);
                RaisePropertyChanged(nameof(BackgroundColor));
            }
        }
        //public int? Value { get; set; }
        public int? NewValue { get; set; }
        private string _displayValue;
        public string DisplayValue
        {
            get { return _displayValue; }
            set { 
                SetProperty(ref _displayValue, value);

                NewValue = string.IsNullOrWhiteSpace(value) ? null : (int?)int.Parse(value);
                if (Value == null)
                {
                    Modified = !string.IsNullOrEmpty(_displayValue);
                }
                else
                {
                    Modified = !Value.Value.ToString().Equals(_displayValue);
                }

                RaisePropertyChanged(nameof(BackgroundColor));
            }
        }
        public string BackgroundColor
        {
            get
            {
                if (!Enabled)
                    return "Gray";
                //return Modified ? "Orange" : "#FF4081";
                if (Value == null)
                {
                    if (string.IsNullOrEmpty(_displayValue))
                        return "#FF4081";
                    else
                        return "Orange";
                }
                else
                {
                    if (Value.Value.ToString().Equals(_displayValue))
                        return "#FF4081";
                    else
                        return "Orange";
                }
            }
        }
        public bool Modified { get; set; }
        public int EntityId { get; set; }
    }
}

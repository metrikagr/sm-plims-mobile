using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class PlateLocation : BindableBase
    {
        public int Position { get; set; }
        private string _color;
        public string Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }
        public string PositionTitle { get; set; }
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        private string _subtitle;
        public string Subtitle
        {
            get { return _subtitle; }
            set { SetProperty(ref _subtitle, value); }
        }
        private ReadingDataEntry _dataType;
        public ReadingDataEntry DataType
        {
            get { return _dataType; }
            set { SetProperty(ref _dataType, value); }
        }
        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }
        public int request_id { get; set; }
        public int sample_type_id { get; set; }
        public int composite_sample_id { get; set; }
        public bool IsSelected { get; set; }
        public int request_process_assay_id { get; set; }
        private string _sampleTypeIndicator;
        public string SampleTypeIndicator
        {
            get { return _sampleTypeIndicator; }
            set { SetProperty(ref _sampleTypeIndicator, value); }
        }
    }
}

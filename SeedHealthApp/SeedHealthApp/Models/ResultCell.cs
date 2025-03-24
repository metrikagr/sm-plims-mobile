using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class ResultCell : Prism.Mvvm.BindableBase
    {
        public int RequestProcessAssayId { get; set; }
        public int CompositeSampleId { get; set; }
        public int ActivityId { get; set; }
        public int AgentId { get; set; }
        //public int? number_result { get; set; }
        public float? NumberResult { get; set; }
        public float? NewNumberResult { get; set; }
        public string AuxiliaryResult { get; set; }
        public string TextResult { get; set; }
        public bool Enabled { get; set; }

        private string _seedsCount;
        public string SeedsCount
        {
            get { return _seedsCount; }
            set
            {
                SetProperty(ref _seedsCount, value);

                NewNumberResult = string.IsNullOrWhiteSpace(value) ? null : (float?)float.Parse(value);
                if (NumberResult == null)
                {
                    Modified = !string.IsNullOrEmpty(_seedsCount);
                }
                else
                {
                    Modified = !NumberResult.Value.ToString().Equals(_seedsCount);
                }

                RaisePropertyChanged(nameof(BackgroundColor));
            }
        }
        private string _textDisplayValue;
        public string TextDisplayValue
        {
            get { return _textDisplayValue; }
            set
            {
                SetProperty(ref _textDisplayValue, value);
                if (string.IsNullOrEmpty(TextResult))
                    Modified = !string.IsNullOrEmpty(value);
                else
                    Modified = !TextResult.Equals(value);
                if (Modified)
                    RaisePropertyChanged(nameof(TextBackgroundColor));
            }
        }
        public string TextBackgroundColor
        {
            get
            {
                if (!Enabled)
                    return "Gray";
                if (Modified)
                    return "Orange";
                else
                    return "#FF4081";
            }
        }
        public string PathogenName { get; set; }
        public int SeedsMaxCount { get; set; }
        public bool Modified { get; set; }
        public string BackgroundColor
        {
            get
            {
                if (!Enabled)
                    return "Gray";
                if (NumberResult == null)
                {
                    if (string.IsNullOrEmpty(_seedsCount))
                        return "#FF4081";
                    else
                        return "Orange";
                }
                else
                {
                    if (NumberResult.Value.ToString().Equals(_seedsCount))
                        return "#FF4081";
                    else
                        return "Orange";
                }
            }
        }

    }
}

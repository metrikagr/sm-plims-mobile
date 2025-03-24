using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class PathogenItem : BindableBase
    {
        [System.Text.Json.Serialization.JsonPropertyName("agent_id")]
        public int PathogenId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("agent_name")]
        public string PathogenName { get; set; }
        
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }
        private string _seedsCount;
        public string SeedsCount
        {
            get { return _seedsCount; }
            set { SetProperty(ref _seedsCount, value); }
        }
        public PathogenItem()
        {
            _isVisible = true;
        }
    }
}

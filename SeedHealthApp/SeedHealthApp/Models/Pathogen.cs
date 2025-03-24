using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class Pathogen : BindableBase
    {
        public string PathogenName { get; set; }
        public int PathogenSeedCount { get; set; }
        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Prism.Mvvm;

namespace SeedHealthApp.Models
{
    public class PcrAgentResult : BindableBase
    {
        public int AgentId { get; set; }
        public string AgentName { get; set; }
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }
    }
}

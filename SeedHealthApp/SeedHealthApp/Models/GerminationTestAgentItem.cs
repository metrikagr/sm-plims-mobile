using System;
using System.Collections.Generic;
using System.Text;
using Prism.Mvvm;

namespace SeedHealthApp.Models
{
    public class GerminationTestAgentItem : BindableBase
    {
        public int AgentId { get; set; }
        public string AgentName { get; set; }

        private string _seedsCount;
        public string SeedsCount
        {
            get { return _seedsCount; }
            set { SetProperty(ref _seedsCount, value); }
        }
    }
}

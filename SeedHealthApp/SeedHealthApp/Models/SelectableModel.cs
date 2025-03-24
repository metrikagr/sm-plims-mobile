using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public class SelectableModel<T> : BindableBase
    {
        public T Item { set; get; }
        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }
    }
}

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace SeedHealthApp.Models
{
    public class ElisaPathogen : BindableBase
    {
        private int _pathogenId;
        public int PathogenId
        {
            get { return _pathogenId; }
            set { SetProperty(ref _pathogenId, value); }
        }
        private string _pathogenName;
        public string PathogenName
        {
            get { return _pathogenName; }
            set { SetProperty(ref _pathogenName, value); }
        }
        private ObservableCollection<ElisaPlate> _plateList;
        public ObservableCollection<ElisaPlate> PlateList
        {
            get { return _plateList; }
            set { SetProperty(ref _plateList, value); }
        }
        private ElisaPlate _selectedElisaPlate;
        public ElisaPlate SelectedElisaPlate
        {
            get { return _selectedElisaPlate; }
            set { SetProperty(ref _selectedElisaPlate, value); }
        }

        public ElisaPathogen()
        {
            _plateList = new ObservableCollection<ElisaPlate>();
        }
    }

    public class ElisaPlate : BindableBase
    {
        public string ElisaPlateCode { get; set; }
        private int _plateOrder;
        public int PlateOrder
        {
            get { return _plateOrder; }
            set { SetProperty(ref _plateOrder, value); }
        }
        private string _thumbnail;
        public string Thumbnail
        {
            get { return _thumbnail; }
            set { SetProperty(ref _thumbnail, value); }
        }
        private ImageSource _thumbnailImageSource;
        public ImageSource ThumbnailImageSource
        {
            get { return _thumbnailImageSource; }
            set { SetProperty(ref _thumbnailImageSource, value); }
        }
        private IEnumerable<PlateCell> _plateCells = Enumerable.Empty<PlateCell>();
        public IEnumerable<PlateCell> PlateCells
        {
            get { return _plateCells; }
            set { SetProperty(ref _plateCells, value); }
        }
    }
}

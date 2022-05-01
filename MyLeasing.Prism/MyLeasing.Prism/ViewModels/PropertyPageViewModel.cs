using MyLeasing.Common.Helpers;
using MyLeasing.Common.Models;
using MyLeasing.Prism.Helpers;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MyLeasing.Prism.ViewModels
{
    public class PropertyPageViewModel : ViewModelBase
    {
        private PropertyResponse _property;
        private ObservableCollection<RotatorModel> _imageCollection;
        private DelegateCommand _editPropertyCommand;
        private readonly INavigationService _navigationService;

        public PropertyPageViewModel(
            INavigationService navigationService) : base(navigationService)
        {
            Title = Languages.Details;
            _navigationService = navigationService;
        }

        public DelegateCommand EditPropertyCommand => _editPropertyCommand ?? (_editPropertyCommand = new DelegateCommand(EditProperty));

        public ObservableCollection<RotatorModel> ImageCollection
        {
            get => _imageCollection;
            set => SetProperty(ref _imageCollection, value);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            Property = JsonConvert.DeserializeObject<PropertyResponse>(Settings.Property);
            LoadImages();
        }

        public PropertyResponse Property
        {
            get => _property;
            set => SetProperty(ref _property, value);
        }

        private void LoadImages()
        {
            var list = new List<RotatorModel>();
            foreach (var propertyImage in Property.PropertyImages)
            {
                list.Add(new RotatorModel { Image = propertyImage.ImageUrl });
            }

            ImageCollection = new ObservableCollection<RotatorModel>(list);
        }
        private async void EditProperty()
        {
            var parameters = new NavigationParameters
            {
                { "property", Property }
            };

            await _navigationService.NavigateAsync("EditPropertyPage",parameters);
        }
    }
}

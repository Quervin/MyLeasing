using MyLeasing.Common.Helpers;
using MyLeasing.Common.Models;
using MyLeasing.Common.Services;
using MyLeasing.Prism.Helpers;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyLeasing.Prism.ViewModels
{
    public class PropertiesPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navagitionService;
        private readonly IApiService _apiService;
        private OwnerResponse _owner;
        private ObservableCollection<PropertyItemViewModel> _properties;
        private DelegateCommand _addPropertyCommand;
        private static PropertiesPageViewModel _instance;

        public PropertiesPageViewModel(
            INavigationService navagitionService,
            IApiService apiService) : base(navagitionService)
        {
            _navagitionService = navagitionService;
            _apiService = apiService;
            _instance = this;
            Title = Languages.Properties;
            LoadOwner();
        }

        public ObservableCollection<PropertyItemViewModel> Properties
        {
            get => _properties;
            set => SetProperty(ref _properties, value);
        }

        public DelegateCommand AddPropertyCommand => _addPropertyCommand ?? (_addPropertyCommand = new DelegateCommand(AddPropertyAsync));

        public static PropertiesPageViewModel GetInstance()
        {
            return _instance;
        }

        public async Task UpdateOwnerAsync()
        {
            var url = App.Current.Resources["UrlAPI"].ToString();
            var token = JsonConvert.DeserializeObject<TokenResponse>(Settings.Token);

            var response = await _apiService.GetOwnerByEmailAsync(
                url,
                "/api",
                "/Owners/GetOwnerByEmail",
                "bearer",
                token.Token,
                _owner.Email);

            if (response.IsSuccess)
            {
                var owner = (OwnerResponse)response.Result;
                Settings.Owner = JsonConvert.SerializeObject(owner);
                _owner = owner;
                LoadOwner();
            }
        }


        private void LoadOwner()
        {
            _owner = JsonConvert.DeserializeObject<OwnerResponse>(Settings.Owner);
            if (_owner.RoleId == 1)
            {
                Title = $"{Languages.Propertiesof} {_owner.FullName}";
            }
            else
            {
                Title = Languages.AvailableProperties;
            }

            Properties = new ObservableCollection<PropertyItemViewModel>(_owner.Properties.Select(p => new PropertyItemViewModel(_navagitionService)
            {
                Address = p.Address,
                Contracts = p.Contracts,
                HasParkingLot = p.HasParkingLot,
                Id = p.Id,
                IsAvailable = p.IsAvailable,
                Neighborhood = p.Neighborhood,
                Price = p.Price,
                PropertyImages = p.PropertyImages,
                PropertyType = p.PropertyType,
                Remarks = p.Remarks,
                Rooms = p.Rooms,
                SquareMeters = p.SquareMeters,
                Stratum = p.Stratum
            }).ToList());
        }

        private async void AddPropertyAsync()
        {
            await _navagitionService.NavigateAsync("EditPropertyPage");
        }
    }
}

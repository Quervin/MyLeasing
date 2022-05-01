using MyLeasing.Common.Helpers;
using MyLeasing.Common.Models;
using MyLeasing.Common.Services;
using MyLeasing.Prism.Helpers;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Navigation;
using System.Threading.Tasks;

namespace MyLeasing.Prism.ViewModels
{
    public class ModifyUserPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly ApiService _apiService;
        private bool _isRunning;
        private bool _isEnabled;
        private OwnerResponse _owner;
        private DelegateCommand _saveCommand;
        private DelegateCommand _changePasswordCommand;

        public ModifyUserPageViewModel(
            INavigationService navigationService,
            ApiService apiService) : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            Title = Languages.ModifyUser;
            IsEnabled = true;
            Owner = JsonConvert.DeserializeObject<OwnerResponse>(Settings.Owner);
        }

        public DelegateCommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand(SaveAsync));
        public DelegateCommand ChangePasswordCommand => _changePasswordCommand ?? (_changePasswordCommand = new DelegateCommand(ChangePasswordAsync));


        public OwnerResponse Owner
        {
            get => _owner;
            set => SetProperty(ref _owner, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private async void SaveAsync()
        {
            var isValid = await ValidateDataAsync();
            if (!isValid)
            {
                return;
            }

            IsRunning = true;
            IsEnabled = false;

            var userRequest = new UserRequest
            {
                Address = Owner.Address,
                Document = Owner.Document,
                Email = Owner.Email,
                FirstName = Owner.FirstName,
                LastName = Owner.LastName,
                Password = "123456", // It doesn't matter what is sent here. It is only for the model to be valid
                Phone = Owner.PhoneNumber
            };

            var token = JsonConvert.DeserializeObject<TokenResponse>(Settings.Token);

            var url = App.Current.Resources["UrlAPI"].ToString();
            var response = await _apiService.PutAsync(
                url,
                "/api",
                "/Account",
                userRequest,
                "bearer",
                token.Token);

            IsRunning = false;
            IsEnabled = true;

            if (!response.IsSuccess)
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    response.Message,
                    Languages.Accept);
                return;
            }

            Settings.Owner = JsonConvert.SerializeObject(Owner);

            await App.Current.MainPage.DisplayAlert(
                Languages.Ok,
                Languages.UserUpdated,
                Languages.Accept);
        }

        private async void ChangePasswordAsync()
        {
            await _navigationService.NavigateAsync("ChangePasswordPage");
        }

        private async Task<bool> ValidateDataAsync()
        {
            if (string.IsNullOrEmpty(Owner.Document))
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.DocumentError,
                    Languages.Accept);
                return false;
            }

            if (string.IsNullOrEmpty(Owner.FirstName))
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.FirstnameError,
                    Languages.Accept);
                return false;
            }

            if (string.IsNullOrEmpty(Owner.LastName))
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.LastnameError,
                    Languages.Accept);
                return false;
            }

            if (string.IsNullOrEmpty(Owner.Address))
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.Address,
                    Languages.Accept);
                return false;
            }

            return true;
        }
    }
}

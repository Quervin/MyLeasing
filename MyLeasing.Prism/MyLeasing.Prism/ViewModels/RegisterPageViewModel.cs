using MyLeasing.Common.Helpers;
using MyLeasing.Common.Models;
using MyLeasing.Common.Services;
using MyLeasing.Prism.Helpers;
using Prism.Commands;
using Prism.Navigation;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyLeasing.Prism.ViewModels
{
    public class RegisterPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private bool _isRunning;
        private bool _isEnabled;
        private DelegateCommand _registerCommand;

        public RegisterPageViewModel(
            INavigationService navigationService,
            IApiService apiService) : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            Title = Languages.RegisterUser;
            IsEnabled = true;
            LoadRoles();
        }

        public DelegateCommand RegisterCommand => _registerCommand ?? (_registerCommand = new DelegateCommand(Register));

        public string Document { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Password { get; set; }

        public string PasswordConfirm { get; set; }

        public Role Role { get; set; }

        public ObservableCollection<Role> Roles { get; set; }

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

        private async void Register()
        {
            var isValid = await ValidateData();
            if (!isValid)
            {
                return;
            }

            IsRunning = true;
            IsEnabled = false;

            var request = new UserRequest
            {
                Address = Address,
                Document = Document,
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                Password = Password,
                Phone = Phone,
                RoleId = Role.Id
            };

            var url = App.Current.Resources["UrlAPI"].ToString();
            var response = await _apiService.RegisterUserAsync(
                url,
                "/api",
                "/Account",
                request);

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

            await App.Current.MainPage.DisplayAlert(
                Languages.Ok, 
                response.Message, 
                Languages.Accept);
            await _navigationService.GoBackAsync();

        }

        private async Task<bool> ValidateData()
        {
            if (string.IsNullOrEmpty(Document))
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.DocumentError,
                    Languages.Accept);
                return false;
            }

            if (string.IsNullOrEmpty(FirstName))
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.FirstnameError,
                    Languages.Accept);
                return false;
            }

            if (string.IsNullOrEmpty(LastName))
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.LastnameError,
                    Languages.Accept);
                return false;
            }

            if (string.IsNullOrEmpty(Address))
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.AddressError,
                    Languages.Accept);
                return false;
            }

            if (string.IsNullOrEmpty(Email) || !RegexHelper.IsValidEmail(Email))
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.EmailError,
                    Languages.Accept);
                return false;
            }

            if (string.IsNullOrEmpty(Phone))
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.PhoneError,
                    Languages.Accept);
                return false;
            }

            if (Role == null)
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.SelectError,
                    Languages.Accept);
                return false;
            }

            if (string.IsNullOrEmpty(Password))
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.PasswordError,
                    Languages.Accept);
                return false;
            }

            if (Password.Length < 6)
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.PasswordLengthError,
                    Languages.Accept);
                return false;
            }

            if (string.IsNullOrEmpty(PasswordConfirm))
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.PasswordConfirmError,
                    Languages.Accept);
                return false;
            }

            if (!Password.Equals(PasswordConfirm))
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.PasswordMatchError,
                    Languages.Accept);
                return false;
            }

            return true;
        }

        private void LoadRoles()
        {
            Roles = new ObservableCollection<Role>
            {
                new Role { Id = 2, Name = "Lessee" },
                new Role { Id = 1, Name = "Owner" }
            };
        }
    }
}


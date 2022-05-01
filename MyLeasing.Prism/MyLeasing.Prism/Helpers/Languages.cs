using MyLeasing.Prism.Interfaces;
using MyLeasing.Prism.Resources;
using Xamarin.Forms;

namespace MyLeasing.Prism.Helpers
{
    public static class Languages
    {
        static Languages()
        {
            var ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
            Resource.Culture = ci;
            DependencyService.Get<ILocalize>().SetLocale(ci);
        }

        public static string Accept => Resource.Accept;
        public static string ConnectionError => Resource.ConnectionError;
        public static string DataError => Resource.DataError;
        public static string Document => Resource.Document;
        public static string DocumentPlaceHolder => Resource.DocumentPlaceHolder;
        public static string Error => Resource.Error;
        public static string EmailError => Resource.EmailError;
        public static string Email => Resource.Email;
        public static string EmailPlaceHolder => Resource.EmailPlaceHolder;
        public static string Password => Resource.Password;
        public static string PasswordPlaceHolder => Resource.PasswordPlaceHolder;
        public static string PasswordError => Resource.PasswordError;
        public static string Rememberme => Resource.Rememberme;
        public static string UserError => Resource.UserError;
        public static string ForgotPassword => Resource.ForgotPassword;
        public static string Login => Resource.Login;
        public static string Register => Resource.Register;
        public static string Loading => Resource.Loading;
        public static string Firstname => Resource.Firstname;
        public static string FirstnamePlaceHolder => Resource.FirstnamePlaceHolder;
        public static string Lastname => Resource.Lastname;
        public static string LastnamePlaceHolder => Resource.LastnamePlaceHolder;
        public static string Address => Resource.Address;
        public static string AddressPlaceHolder => Resource.AddressPlaceHolder;
        public static string Phone => Resource.Phone;
        public static string PhonePlaceHolder => Resource.PhonePlaceHolder;
        public static string Registeras => Resource.Registeras;
        public static string PasswordConfirm => Resource.PasswordConfirm;
        public static string PasswordConfirmPlaceHolder => Resource.PasswordConfirmPlaceHolder;
        public static string Registering => Resource.Registering;
        public static string Ok => Resource.Ok;
        public static string DocumentError => Resource.DocumentError;
        public static string FirstnameError => Resource.FirstnameError;
        public static string LastnameError => Resource.LastnameError;
        public static string AddressError => Resource.AddressError;
        public static string PhoneError => Resource.PhoneError;
        public static string SelectError => Resource.SelectError;
        public static string PasswordLengthError => Resource.PasswordLengthError;
        public static string PasswordConfirmError => Resource.PasswordConfirmError;
        public static string PasswordMatchError => Resource.PasswordMatchError;
        public static string RegisterUser => Resource.RegisterUser;
        public static string Map => Resource.Map;
        public static string CurrentPassword => Resource.CurrentPassword;
        public static string CurrentPasswordPlaceHolder => Resource.CurrentPasswordPlaceHolder;
        public static string NewPassword => Resource.NewPassword;
        public static string NewPasswordPlaceHolder => Resource.NewPasswordPlaceHolder;
        public static string ConfirmNewPassword => Resource.ConfirmNewPassword;
        public static string ConfirmNewPasswordPlaceHolder => Resource.ConfirmNewPasswordPlaceHolder;
        public static string ChangePassword => Resource.ChangePassword;
        public static string Saving => Resource.Saving;
        public static string CurrentPasswordError => Resource.CurrentPasswordError;
        public static string NewPasswordLengthError => Resource.NewPasswordLengthError;
        public static string StartDate => Resource.StartDate;
        public static string EndDate => Resource.EndDate;
        public static string Lessee => Resource.Lessee;
        public static string Contracts => Resource.Contracts;
        public static string Price => Resource.Price;
        public static string IsActive => Resource.IsActive;
        public static string Remarks => Resource.Remarks;
        public static string Contract => Resource.Contract;
        public static string Save => Resource.Save;
        public static string ModifyUser => Resource.ModifyUser;
        public static string UserUpdated => Resource.UserUpdated;
        public static string Neighborhood => Resource.Neighborhood;
        public static string PropertyType => Resource.PropertyType;
        public static string IsAvailable => Resource.IsAvailable;
        public static string Properties => Resource.Properties;
        public static string AvailableProperties => Resource.AvailableProperties;
        public static string Propertiesof => Resource.Propertiesof;
        public static string HasParkingLot => Resource.HasParkingLot;
        public static string SquareMeters => Resource.SquareMeters;
        public static string Rooms => Resource.Rooms;
        public static string Stratum => Resource.Stratum;
        public static string Details => Resource.Details;
        public static string Property => Resource.Property;
        public static string Logout => Resource.Logout;
        public static string Recoverpassword => Resource.Recoverpassword;
        public static string Recovering => Resource.Recovering;
        public static string ErrorNoOwner => Resource.ErrorNoOwner;
        public static string EditProperty => Resource.EditProperty;
        public static string AddProperty => Resource.AddProperty;
        public static string Delete => Resource.Delete;
        public static string ChangeImage => Resource.ChangeImage;
        public static string NeighborhoodError => Resource.NeighborhoodError;
        public static string NeighborhoodPlaceHolder => Resource.NeighborhoodPlaceHolder;
        public static string PriceError => Resource.PriceError;
        public static string PricePlaceHolder => Resource.PricePlaceHolder;
        public static string SquareMetersError => Resource.SquareMetersError;
        public static string SquareMetersPlaceHolder => Resource.SquareMetersPlaceHolder;
        public static string RoomsError => Resource.RoomsError;
        public static string RoomsPlaceHolder => Resource.RoomsPlaceHolder;
        public static string StratumError => Resource.StratumError;
        public static string StratumPlaceHolder => Resource.StratumPlaceHolder;
        public static string PropertyTypeError => Resource.PropertyTypeError;
        public static string PropertyTypePlaceHolder => Resource.PropertyTypePlaceHolder;
        public static string AddImage => Resource.AddImage;
        public static string DeleteImage => Resource.DeleteImage;
        public static string PictureSource => Resource.PictureSource;

        public static string Cancel => Resource.Cancel;

        public static string FromCamera => Resource.FromCamera;

        public static string FromGallery => Resource.FromGallery;

        public static string CreateEditPropertyConfirm => Resource.CreateEditPropertyConfirm;

        public static string Created => Resource.Created;

        public static string Edited => Resource.Edited;

    }

}

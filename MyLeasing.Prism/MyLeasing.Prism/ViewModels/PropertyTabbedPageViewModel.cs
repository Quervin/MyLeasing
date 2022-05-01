using MyLeasing.Common.Helpers;
using MyLeasing.Common.Models;
using MyLeasing.Prism.Helpers;
using Newtonsoft.Json;
using Prism.Navigation;

namespace MyLeasing.Prism.ViewModels
{
    public class PropertyTabbedPageViewModel : ViewModelBase
    {
        public PropertyTabbedPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            var property = JsonConvert.DeserializeObject<PropertyResponse>(Settings.Property);
            Title = $"{Languages.Property} {property.Neighborhood}";
        }
    }
}

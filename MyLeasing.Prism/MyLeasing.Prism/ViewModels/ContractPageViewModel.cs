using MyLeasing.Common.Models;
using MyLeasing.Prism.Helpers;
using Prism.Navigation;

namespace MyLeasing.Prism.ViewModels
{
    public class ContractPageViewModel : ViewModelBase
    {
        private ContractResponse _contract;
        public ContractPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            Title = Languages.Contract;
        }

        public ContractResponse Contract
        {
            get => _contract;
            set => SetProperty(ref _contract, value);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("contract"))
            {
                Contract = parameters.GetValue<ContractResponse>("contract");
            }
        }
    }
}

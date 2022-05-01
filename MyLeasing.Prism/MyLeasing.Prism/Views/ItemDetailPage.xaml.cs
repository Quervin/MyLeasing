using MyLeasing.Prism.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace MyLeasing.Prism.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}
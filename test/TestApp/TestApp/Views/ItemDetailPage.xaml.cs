using System.ComponentModel;
using TestApp.ViewModels;
using Xamarin.Forms;

namespace TestApp.Views
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
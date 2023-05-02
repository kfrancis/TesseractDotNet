using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace TestApp.ViewModels
{
    public partial class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
        }

        [RelayCommand]
        public Task OpenWebAsync()
        {
            return Browser.OpenAsync("https://aka.ms/xamarin-quickstart");
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TestApp.Models;
using TestApp.Services;
using Xamarin.Forms;

namespace TestApp.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        public IDataStore<Item> DataStore => DependencyService.Get<IDataStore<Item>>();

        [ObservableProperty]
        private bool _isBusy = false;

        [ObservableProperty]
        private string _title = string.Empty;
    }
}

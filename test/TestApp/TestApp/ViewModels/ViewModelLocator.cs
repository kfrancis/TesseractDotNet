using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace TestApp.ViewModels
{
    public static class ViewModelLocator
    { 

        public static T Resolve<T>() where T : class => DependencyService.Resolve<T>();
    }
}

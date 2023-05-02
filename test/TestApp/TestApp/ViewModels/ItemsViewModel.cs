using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using TesseractDotNet;
using TestApp.Models;
using TestApp.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TestApp.ViewModels
{
    public partial class ItemsViewModel : BaseViewModel
    {
        private Item _selectedItem;

        private readonly ITesseractApi _tesseract;

        [ObservableProperty]
        private string _text;

        public ItemsViewModel()
        {
            Title = "Browse";
            Items = new ObservableCollection<Item>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<Item>(OnItemSelected);

            AddItemCommand = new Command(OnAddItem);

            _tesseract = ViewModelLocator.Resolve<ITesseractApi>();
        }

        public Command AddItemCommand { get; }
        public ObservableCollection<Item> Items { get; }
        public Command<Item> ItemTapped { get; }
        public Command LoadItemsCommand { get; }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

        [RelayCommand]
        public async Task PickPhotoAsync()
        {
            try
            {
                IsBusy = true;
                var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions() { });
                await Recognize(photo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task TakePhotoAsync()
        {
            try
            {
                IsBusy = true;
                var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions() { });
                await Recognize(photo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnAddItem(object obj)
        {
            await Shell.Current.GoToAsync(nameof(NewItemPage));
        }

        private async void OnItemSelected(Item item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
        }

        private async Task Recognize(FileResult result)
        {
            try
            {
                IsBusy = true;

                if (!_tesseract.Initialized)
                {
                    var initialised = await _tesseract.Init("eng");
                    if (!initialised)
                        return;
                }

                if (!await _tesseract.SetImage(result.FullPath))
                    return;

                Text = _tesseract.Text;

                var words = _tesseract.Results(PageIteratorLevel.Word);
                var symbols = _tesseract.Results(PageIteratorLevel.Symbol);
                var blocks = _tesseract.Results(PageIteratorLevel.Block);
                var paragraphs = _tesseract.Results(PageIteratorLevel.Paragraph);
                var lines = _tesseract.Results(PageIteratorLevel.Textline);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
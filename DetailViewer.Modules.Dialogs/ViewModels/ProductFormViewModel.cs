using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class ProductFormViewModel : BindableBase, IDialogAware
    {
        private readonly IDocumentDataService _documentDataService;

        public string Title => "Форма изделия";

        public event Action<IDialogResult> RequestClose;

        private Product _product;
        public Product Product
        {
            get { return _product; }
            set { SetProperty(ref _product, value); }
        }

        // ESKD Number Properties
        private string _companyCode, _classNumberString;
        private int _detailNumber;
        private int? _version;

        public string CompanyCode { get => _companyCode; set => SetProperty(ref _companyCode, value, OnESKDNumberPartChanged); }
        public string ClassNumberString { get => _classNumberString; set => SetProperty(ref _classNumberString, value, OnESKDNumberPartChanged); }
        public int DetailNumber { get => _detailNumber; set => SetProperty(ref _detailNumber, value, OnESKDNumberPartChanged); }
        public int? Version { get => _version; set => SetProperty(ref _version, value, OnESKDNumberPartChanged); }

        public string ESKDNumberString
        {
            get
            {
                if (string.IsNullOrEmpty(CompanyCode) || string.IsNullOrEmpty(ClassNumberString) || DetailNumber == 0)
                {
                    return string.Empty;
                }

                string baseCode = $"{CompanyCode}.{int.Parse(ClassNumberString):D6}.{DetailNumber:D3}";
                return Version.HasValue ? $"{baseCode}-{Version.Value:D2}" : baseCode;
            }
        }

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public ProductFormViewModel(IDocumentDataService documentDataService)
        {
            _documentDataService = documentDataService;

            Product = new Product
            {
                EskdNumber = new ESKDNumber()
            };

            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
        }

        private void OnESKDNumberPartChanged() => RaisePropertyChanged(nameof(ESKDNumberString));

        private async void Save()
        {
            Product.EskdNumber.CompanyCode = CompanyCode;
            if (int.TryParse(ClassNumberString, out int classNumber))
            {
                Product.EskdNumber.ClassNumber = new Classifier { Number = classNumber };
            }
            Product.EskdNumber.DetailNumber = DetailNumber;
            Product.EskdNumber.Version = Version;

            if (Product.Id == 0)
            {
                await _documentDataService.AddProductAsync(Product);
            }
            else
            {
                await _documentDataService.UpdateProductAsync(Product);
            }

            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        private void Cancel() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("product"))
            {
                Product = parameters.GetValue<Product>("product");
                CompanyCode = Product.EskdNumber.CompanyCode;
                ClassNumberString = Product.EskdNumber.ClassNumber?.Number.ToString("D6");
                DetailNumber = Product.EskdNumber.DetailNumber;
                Version = Product.EskdNumber.Version;
            }
        }
    }
}
